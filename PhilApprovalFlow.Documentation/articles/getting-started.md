# Getting Started with PhilApprovalFlow

Welcome to PhilApprovalFlow! This guide will help you get up and running with approval workflows in your .NET application in just a few minutes.

## Installation

### Package Manager Console

```powershell
Install-Package PhilApprovalFlow -Version 1.2.14
```

### .NET CLI

```bash
dotnet add package PhilApprovalFlow --version 1.2.14
```

### PackageReference

Add this to your `.csproj` file:

```xml
<PackageReference Include="PhilApprovalFlow" Version="1.2.14" />
```

## Quick Start Example

Let's create a simple invoice approval workflow:

### 1. Create Your Models

```csharp
using PhilApprovalFlow;
using PhilApprovalFlow.Models;
using System;
using System.Collections.Generic;

// Your transition model
public class InvoiceTransition : PAFTransition, IPAFTransition
{
    // Add custom properties if needed
    public string InvoiceNumber { get; set; }
}

// Your entity that needs approval
public class Invoice : IApprovalFlow<InvoiceTransition>
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; }
    public decimal Amount { get; set; }
    public string Vendor { get; set; }
    public string Description { get; set; }
    
    // Required by IApprovalFlow
    public ICollection<InvoiceTransition> Transitions { get; set; }
    
    public Invoice()
    {
        Id = Guid.NewGuid();
        Transitions = new HashSet<InvoiceTransition>();
    }
    
    // Required method implementations
    public object GetID() => Id;
    public string GetShortDescription() => $"Invoice {InvoiceNumber} - {Amount:C}";
    public string GetLongDescription() => $"Invoice {InvoiceNumber} from {Vendor} for {Description} - Amount: {Amount:C}";
}
```

### 2. Basic Workflow Operations

```csharp
using PhilApprovalFlow;
using PhilApprovalFlow.Enum;

class Program
{
    static void Main()
    {
        // Create an invoice
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-001",
            Amount = 5000m,
            Vendor = "Acme Corp",
            Description = "Office Supplies"
        };
        
        // Request approval workflow
        RequestApproval(invoice);
        
        // Manager approves
        ApproveInvoice(invoice);
        
        // Check status
        CheckApprovalStatus(invoice);
    }
    
    static void RequestApproval(Invoice invoice)
    {
        var workflow = invoice.GetApprovalFlow();
        
        // Employee requests approval from manager
        workflow.SetUserName("employee@company.com")
               .RequestApproval("manager@company.com", "Manager", 
                               "Please approve this invoice for office supplies");
        
        Console.WriteLine("✓ Approval requested from manager");
    }
    
    static void ApproveInvoice(Invoice invoice)
    {
        var workflow = invoice.GetApprovalFlow();
        
        // Manager approves the invoice
        workflow.SetUserName("manager@company.com")
               .Approve("Approved - expense is justified");
        
        Console.WriteLine("✓ Invoice approved by manager");
    }
    
    static void CheckApprovalStatus(Invoice invoice)
    {
        bool isApproved = invoice.Transitions.IsApproved();
        bool hasPendingDecisions = invoice.Transitions.IsAnyDecisionPending();
        
        Console.WriteLine($"Invoice Status:");
        Console.WriteLine($"  - Fully Approved: {isApproved}");
        Console.WriteLine($"  - Has Pending Decisions: {hasPendingDecisions}");
        
        // Print all transitions
        foreach (var transition in invoice.Transitions)
        {
            Console.WriteLine($"  - {transition.ApproverRole}: {transition.ApproverDecision}");
            if (!string.IsNullOrEmpty(transition.ApproverComments))
                Console.WriteLine($"    Comment: {transition.ApproverComments}");
        }
    }
}
```

## Working with Groups

Here's how to set up group-based approvals:

```csharp
using PhilApprovalFlow.Models;

static void RequestGroupApproval(Invoice invoice)
{
    // Create a finance approval group
    var financeGroup = new PAFApproverGroup { GroupID = 1 };
    financeGroup.SetApprovers(new[] 
    {
        "finance1@company.com",
        "finance2@company.com",
        "finance3@company.com"
    });
    financeGroup.SetActiveStatus(true);
    
    var workflow = invoice.GetApprovalFlow();
    workflow.SetUserName("employee@company.com")
           .RequestApproval(financeGroup, "Finance Team", 
                           "Finance approval required for this invoice");
    
    Console.WriteLine("✓ Approval requested from finance team");
}

static void GroupMemberApproves(Invoice invoice)
{
    var workflow = invoice.GetApprovalFlow();
    
    // Any member of the finance group can approve
    workflow.SetUserName("finance1@company.com")
           .Approve("Budget verified and approved");
    
    Console.WriteLine("✓ Invoice approved by finance team member");
}
```

## Handling Notifications

```csharp
using PhilApprovalFlow.Enum;

static void SetupNotifications(Invoice invoice)
{
    var workflow = invoice.GetApprovalFlow();
    
    // Load notification for the approver
    workflow.SetUserName("employee@company.com")
           .LoadNotification("manager@company.com", 
                           usersToCC: new[] { "admin@company.com" },
                           mailsToCC: new[] { "notifications@company.com" });
    
    // Get all pending notifications
    var notifications = workflow.GetPAFNotifications();
    
    foreach (var notification in notifications)
    {
        Console.WriteLine($"Notification:");
        Console.WriteLine($"  From: {notification.From}");
        Console.WriteLine($"  To: {notification.To}");
        Console.WriteLine($"  Type: {notification.DecisionType}");
        Console.WriteLine($"  Comments: {notification.Comments}");
        
        // Here you would send the actual notification
        // SendEmail(notification);
    }
    
    // Clear notifications to prevent duplicates
    workflow.ClearNotifications();
}
```

## Multiple Approvals Scenario

Here's a more complex example with sequential approvals:

```csharp
static void MultiLevelApproval(Invoice invoice)
{
    var workflow = invoice.GetApprovalFlow();
    
    // Set up multi-level approval based on amount
    workflow.SetUserName("employee@company.com");
    
    if (invoice.Amount > 10000)
    {
        // High-value invoices need multiple approvals
        workflow.RequestApproval("supervisor@company.com", "Supervisor")
               .RequestApproval("manager@company.com", "Manager")
               .RequestApproval("director@company.com", "Director");
    }
    else if (invoice.Amount > 1000)
    {
        // Medium-value invoices need manager approval
        workflow.RequestApproval("manager@company.com", "Manager");
    }
    else
    {
        // Low-value invoices need supervisor approval
        workflow.RequestApproval("supervisor@company.com", "Supervisor");
    }
    
    Console.WriteLine($"✓ Approval workflow set up for {invoice.Amount:C} invoice");
}

static void ProcessApprovals(Invoice invoice)
{
    var workflow = invoice.GetApprovalFlow();
    
    // Supervisor approves first
    if (invoice.Transitions.IsInTransitions("supervisor@company.com"))
    {
        workflow.SetUserName("supervisor@company.com")
               .Approve("Supervisor approval completed");
        Console.WriteLine("✓ Supervisor approved");
    }
    
    // Manager approves second
    if (invoice.Transitions.IsInTransitions("manager@company.com"))
    {
        workflow.SetUserName("manager@company.com")
               .Approve("Manager approval completed");
        Console.WriteLine("✓ Manager approved");
    }
    
    // Director approves last
    if (invoice.Transitions.IsInTransitions("director@company.com"))
    {
        workflow.SetUserName("director@company.com")
               .Approve("Director approval completed");
        Console.WriteLine("✓ Director approved");
    }
}
```

## Using Metadata

```csharp
static void WorkingWithMetadata(Invoice invoice)
{
    var workflow = invoice.GetApprovalFlow();
    
    // Set custom metadata
    workflow.SetUserName("employee@company.com")
           .SetMetadata("Priority", "High")
           .SetMetadata("Department", "IT")
           .SetMetadata("BudgetCode", "IT-2024-Q1")
           .SetEntityMetaData(); // This sets id, shortDescription, longDescription
    
    // Retrieve metadata
    try
    {
        string priority = workflow.GetMetadata("Priority");
        string department = workflow.GetMetadata("Department");
        string entityId = workflow.GetMetadata("id");
        string shortDesc = workflow.GetMetadata("shortDescription");
        
        Console.WriteLine($"Invoice Metadata:");
        Console.WriteLine($"  Priority: {priority}");
        Console.WriteLine($"  Department: {department}");
        Console.WriteLine($"  Entity ID: {entityId}");
        Console.WriteLine($"  Description: {shortDesc}");
    }
    catch (KeyNotFoundException ex)
    {
        Console.WriteLine($"Metadata key not found: {ex.Message}");
    }
}
```

## Check-in Functionality

```csharp
static void DemonstrateCheckIn(Invoice invoice)
{
    var workflow = invoice.GetApprovalFlow();
    
    // Request approval
    workflow.SetUserName("employee@company.com")
           .RequestApproval("manager@company.com", "Manager");
    
    // Manager checks in (indicates they've seen the request)
    workflow.SetUserName("manager@company.com")
           .CheckIn();
    
    // Check if manager has checked in
    bool isCheckedIn = invoice.Transitions.IsCheckedIn("manager@company.com");
    Console.WriteLine($"Manager checked in: {isCheckedIn}");
    
    // Later, manager makes a decision
    workflow.SetUserName("manager@company.com")
           .Approve("Reviewed and approved");
    
    bool hasDecision = invoice.Transitions.IsTakenDecision("manager@company.com");
    Console.WriteLine($"Manager has made decision: {hasDecision}");
}
```

## Error Handling

```csharp
static void ErrorHandlingExample(Invoice invoice)
{
    try
    {
        var workflow = invoice.GetApprovalFlow();
        
        // This will throw an exception if no transition exists
        workflow.SetUserName("nonexistent@company.com")
               .Approve();
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Workflow error: {ex.Message}");
    }
    catch (ArgumentNullException ex)
    {
        Console.WriteLine($"Validation error: {ex.Message}");
    }
    catch (KeyNotFoundException ex)
    {
        Console.WriteLine($"Metadata error: {ex.Message}");
    }
}
```

## Best Practices for Getting Started

### 1. Always Initialize Collections
```csharp
public Invoice()
{
    Transitions = new HashSet<InvoiceTransition>();
}
```

### 2. Use Meaningful Roles
```csharp
workflow.RequestApproval("user@company.com", "Finance Manager", "Budget verification needed");
```

### 3. Add Helpful Comments
```csharp
workflow.Approve("Approved after verifying against Q1 budget allocation");
```

### 4. Check States Before Actions
```csharp
if (invoice.Transitions.IsApprovedEnabled("user@company.com"))
{
    workflow.Approve();
}
```

### 5. Handle Rejections
```csharp
if (transitions.Any(t => t.ApproverDecision == DecisionType.Rejected))
{
    // Handle rejection logic
    Console.WriteLine("Invoice was rejected and needs revision");
}
```

## Next Steps

Now that you have the basics down, you can:

1. **Integrate with a Database**: Use Entity Framework to persist your workflows
2. **Add Email Notifications**: Implement email sending based on the notification system
3. **Build a UI**: Create views for users to manage their approvals
4. **Add Authentication**: Integrate with your application's user management
5. **Customize Workflows**: Explore advanced scenarios and custom business rules

For more detailed information, check out our [Implementation Guide](../articles/implementation.md) and [API Documentation](../api/index.md).

## Sample Project

For a complete working example, check out our [sample project on GitHub](https://github.com/vish-j/PhilApprovalFlow/tree/main/samples) that demonstrates all these concepts in a real application.