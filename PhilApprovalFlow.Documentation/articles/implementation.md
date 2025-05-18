# Implementation Guide

This guide walks you through implementing PhilApprovalFlow in your .NET application, from basic setup to advanced scenarios.

## Prerequisites

- .NET Framework 4.8+ or .NET 8+
- Basic understanding of C# interfaces and generics
- Entity Framework (optional, for persistence)

## Step 1: Define Your Transition Model

First, create a transition class that inherits from `PAFTransition`:

```csharp
using PhilApprovalFlow.Models;

public class DocumentTransition : PAFTransition, IPAFTransition
{
    // You can add custom properties specific to your domain
    public string DocumentType { get; set; }
    public decimal? Amount { get; set; }
    public string Department { get; set; }
}
```

## Step 2: Implement IApprovalFlow in Your Entity

Make your entity implement the `IApprovalFlow<T>` interface:

```csharp
using PhilApprovalFlow;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Document : IApprovalFlow<DocumentTransition>
{
    [Key]
    public int DocumentId { get; set; }
    
    public string Title { get; set; }
    public string Content { get; set; }
    public string Author { get; set; }
    public decimal Amount { get; set; }
    
    // Required by IApprovalFlow
    public virtual ICollection<DocumentTransition> Transitions { get; set; }
    
    public Document()
    {
        Transitions = new HashSet<DocumentTransition>();
    }
    
    // Required method implementations
    public object GetID() => DocumentId;
    
    public string GetShortDescription() => $"Document: {Title}";
    
    public string GetLongDescription() => 
        $"Document '{Title}' by {Author} - Amount: {Amount:C}";
}
```

## Step 3: Database Configuration (Optional)

If using Entity Framework, configure your DbContext:

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentTransition> DocumentTransitions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the relationship
        modelBuilder.Entity<Document>()
            .HasMany(d => d.Transitions)
            .WithOne()
            .HasForeignKey("DocumentId");
            
        // Configure DocumentTransition
        modelBuilder.Entity<DocumentTransition>()
            .HasKey(t => t.TransitionID);
            
        base.OnModelCreating(modelBuilder);
    }
}
```

## Step 4: Basic Workflow Implementation

Now you can use the fluent API to manage approvals:

```csharp
public class DocumentService
{
    private readonly ApplicationDbContext _context;
    
    public DocumentService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Document> CreateDocumentAsync(Document document, string authorId)
    {
        // Save the document
        _context.Documents.Add(document);
        await _context.SaveChangesAsync();
        
        // Request approval from manager
        var workflow = document.GetApprovalFlow();
        workflow.SetUserName(authorId)
               .RequestApproval("manager@company.com", "Manager", 
                               "Please review this document")
               .SetEntityMetaData(); // Automatically set entity metadata
        
        await _context.SaveChangesAsync();
        return document;
    }
    
    public async Task ApproveDocumentAsync(int documentId, string approverId, string comments = null)
    {
        var document = await _context.Documents
            .Include(d => d.Transitions)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);
            
        if (document == null) return;
        
        var workflow = document.GetApprovalFlow();
        workflow.SetUserName(approverId)
               .Approve(comments);
        
        await _context.SaveChangesAsync();
    }
    
    public async Task RejectDocumentAsync(int documentId, string approverId, string comments)
    {
        var document = await _context.Documents
            .Include(d => d.Transitions)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);
            
        if (document == null) return;
        
        var workflow = document.GetApprovalFlow();
        workflow.SetUserName(approverId)
               .Reject(comments);
        
        await _context.SaveChangesAsync();
    }
}
```

## Step 5: Working with Approver Groups

For group-based approvals:

```csharp
public class GroupApprovalService
{
    public void SetupFinanceApproval(Document document, string requesterId)
    {
        // Create approver group
        var financeTeam = new PAFApproverGroup { GroupID = 1 };
        financeTeam.SetApprovers(new[] 
        {
            "finance.manager@company.com",
            "finance.director@company.com",
            "cfo@company.com"
        });
        financeTeam.SetActiveStatus(true);
        
        // Request approval from the group
        var workflow = document.GetApprovalFlow();
        workflow.SetUserName(requesterId)
               .RequestApproval(financeTeam, "Finance Team", 
                               "Requires finance approval for budget allocation");
    }
    
    public void ApproveAsGroupMember(Document document, string approverId)
    {
        var workflow = document.GetApprovalFlow();
        workflow.SetUserName(approverId)
               .Approve("Approved by finance team member");
    }
}
```

## Step 6: Notifications

Generate and handle notifications:

```csharp
public class NotificationService
{
    public async Task SendApprovalNotificationsAsync(Document document, string requesterId)
    {
        var workflow = document.GetApprovalFlow();
        
        // Load notifications for all pending approvers
        workflow.SetUserName(requesterId);
        
        foreach (var transition in document.Transitions.Where(t => 
            t.ApproverDecision == DecisionType.AwaitingDecision))
        {
            if (transition.ApproverID != null)
            {
                workflow.LoadNotification(transition.ApproverID, 
                    usersToCC: new[] { "admin@company.com" },
                    mailsToCC: new[] { "notifications@company.com" });
            }
            else if (transition.ApproverGroup != null)
            {
                workflow.LoadNotification(transition.ApproverGroup);
            }
        }
        
        // Get all notifications and send them
        var notifications = workflow.GetPAFNotifications();
        foreach (var notification in notifications)
        {
            await SendEmailNotificationAsync(notification);
        }
        
        // Clear notifications to prevent duplicates
        workflow.ClearNotifications();
    }
    
    private async Task SendEmailNotificationAsync(IPAFNotification notification)
    {
        // Implement your email sending logic here
        var subject = GetEmailSubject(notification.DecisionType);
        var body = BuildEmailBody(notification);
        
        // Send email using your preferred email service
        // await _emailService.SendAsync(notification.From, notification.To, subject, body);
    }
    
    private string GetEmailSubject(DecisionType decisionType)
    {
        return decisionType switch
        {
            DecisionType.AwaitingDecision => "Approval Required",
            DecisionType.Approved => "Request Approved",
            DecisionType.Rejected => "Request Rejected",
            DecisionType.Invalidated => "Approval Request Cancelled",
            _ => "Workflow Notification"
        };
    }
}
```

## Step 7: Querying Workflow States

Use extension methods to query workflow states:

```csharp
public class WorkflowQueryService
{
    public DocumentStatus GetDocumentStatus(Document document)
    {
        var transitions = document.Transitions;
        
        if (!transitions.Any())
            return DocumentStatus.Draft;
            
        if (transitions.IsApproved())
            return DocumentStatus.Approved;
            
        if (transitions.Any(t => t.ApproverDecision == DecisionType.Rejected))
            return DocumentStatus.Rejected;
            
        if (transitions.IsAnyDecisionPending())
            return DocumentStatus.PendingApproval;
            
        return DocumentStatus.Unknown;
    }
    
    public List<Document> GetDocumentsPendingApproval(string approverId)
    {
        return _context.Documents
            .Include(d => d.Transitions)
            .Where(d => d.Transitions.IsInTransitions(approverId) &&
                       d.Transitions.IsApprovedEnabled(approverId))
            .ToList();
    }
    
    public bool CanUserApprove(Document document, string userId)
    {
        return document.Transitions.IsInTransitions(userId) &&
               document.Transitions.IsApprovedEnabled(userId);
    }
}

public enum DocumentStatus
{
    Draft,
    PendingApproval,
    Approved,
    Rejected,
    Unknown
}
```

## Step 8: Advanced Scenarios

### Sequential Approvals

```csharp
public void SetupSequentialApproval(Document document, string requesterId)
{
    var workflow = document.GetApprovalFlow();
    workflow.SetUserName(requesterId)
           .RequestApproval("supervisor@company.com", "Supervisor")
           .RequestApproval("manager@company.com", "Manager")
           .RequestApproval("director@company.com", "Director");
}
```

### Conditional Approvals

```csharp
public void SetupConditionalApproval(Document document, string requesterId)
{
    var workflow = document.GetApprovalFlow();
    workflow.SetUserName(requesterId);
    
    // Different approval chains based on amount
    if (document.Amount < 1000)
    {
        workflow.RequestApproval("supervisor@company.com", "Supervisor");
    }
    else if (document.Amount < 10000)
    {
        workflow.RequestApproval("manager@company.com", "Manager");
    }
    else
    {
        workflow.RequestApproval("director@company.com", "Director")
               .RequestApproval("ceo@company.com", "CEO");
    }
}
```

### Metadata Usage

```csharp
public void SetCustomMetadata(Document document, string requesterId)
{
    var workflow = document.GetApprovalFlow();
    workflow.SetUserName(requesterId)
           .SetMetadata("Priority", "High")
           .SetMetadata("DueDate", DateTime.Now.AddDays(7).ToString())
           .SetMetadata("CostCenter", "IT-001")
           .SetEntityMetaData(); // Also set entity metadata
    
    // Retrieve metadata later
    string priority = workflow.GetMetadata("Priority");
    string entityId = workflow.GetMetadata("id");
}
```

## Best Practices

1. **Always Include Error Handling**: Wrap workflow operations in try-catch blocks
2. **Use Transactions**: Ensure data consistency when persisting workflows
3. **Implement Proper Authorization**: Verify users can perform requested actions
4. **Log Workflow Changes**: Maintain audit trails for compliance
5. **Handle Concurrent Updates**: Implement optimistic concurrency control
6. **Test Thoroughly**: Create unit tests for all workflow scenarios

## Troubleshooting

### Common Issues

1. **InvalidOperationException**: No transition found
   - Ensure the user has a transition before calling Approve/Reject
   - Check if the approver is part of an active group

2. **ArgumentNullException**: Null parameters
   - Validate input parameters before calling workflow methods
   - Ensure entities are properly initialized

3. **KeyNotFoundException**: Metadata not found
   - Check if the metadata key exists before accessing
   - Use TryGetValue pattern for safe access

### Performance Considerations

- Use `Include()` to eagerly load transitions when querying entities
- Consider using projection queries for read-only operations
- Implement caching for frequently accessed approval groups
- Use async methods for database operations

This completes the basic implementation guide. Your approval workflow should now be fully functional and ready for production use.