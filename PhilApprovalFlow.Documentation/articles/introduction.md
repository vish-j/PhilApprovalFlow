# Introduction to PhilApprovalFlow

Welcome to PhilApprovalFlow, a powerful and flexible approval workflow library for .NET applications. This library is designed to simplify the implementation of complex approval processes in your business applications.

## What is PhilApprovalFlow?

PhilApprovalFlow is a comprehensive approval workflow framework designed to streamline and automate approval processes in .NET applications. Whether you're building enterprise applications, document management systems, or any application requiring structured approval workflows, PhilApprovalFlow provides the tools you need.

This business logic tool handles approval-related workflows for any entity in your application, offering a fluent API that makes it easy to:

- **Attach approval workflows to any entity** in your application
- **Track approval status** through various decision states
- **Support both individual and group approvers**
- **Generate notifications** for workflow participants
- **Maintain complete audit trails** of approval decisions

## Why Choose PhilApprovalFlow?

### 🚀 Developer-Friendly
- **Fluent API**: Intuitive, readable code that flows naturally
- **Strong Typing**: Compile-time safety with full IntelliSense support
- **Minimal Configuration**: Get started with just a few lines of code
- **Well Documented**: Comprehensive XML documentation for all APIs

### 🏢 Business-Ready
- **Flexible Workflows**: Support for simple to complex approval chains
- **Group Approvals**: Any member of a group can approve requests
- **State Management**: Automatic tracking of all workflow states
- **Audit Trail**: Complete history of who did what and when

### 🔧 Technical Excellence
- **.NET Standard 2.0**: Compatible with .NET Framework 4.8+ and .NET 8+
- **Framework Agnostic**: Works with any .NET application architecture
- **Entity Framework Ready**: Seamless integration with EF Core/Framework
- **Thread Safe**: Designed for concurrent access scenarios

## Core Concepts

### 1. Approval Entities
Any class can become an approval entity by implementing the `IApprovalFlow<T>` interface, where `T` is your custom transition type that inherits from `PAFTransition`.

```csharp
public class PurchaseOrder : IApprovalFlow<PurchaseOrderTransition>
{
    public ICollection<PurchaseOrderTransition> Transitions { get; set; }
    public object GetID() => Id;
    public string GetShortDescription() => $"PO #{Number}";
    public string GetLongDescription() => $"Purchase Order #{Number} - {Description}";
}
```

### 2. Transitions
**Transitions** represent individual approval steps, each containing:
- Approver information (user ID or group)
- Decision status (Awaiting, Approved, Rejected, Invalidated)
- Comments from both requester and approver
- Timestamps for audit purposes
- Role information for context

### 3. Fluent API Workflow
The library provides an intuitive fluent interface that reads like natural language:

```csharp
// Request approval
entity.GetApprovalFlow()
      .SetUserName("requester@company.com")
      .RequestApproval("manager@company.com", "Manager", "Please review this order");

// Approve the request
entity.GetApprovalFlow()
      .SetUserName("manager@company.com")
      .Approve("Approved - within budget");
```

### 4. Decision States
Each transition can be in one of four states:

- **AwaitingDecision**: Initial state, waiting for approver action
- **Approved**: Approver has approved the request
- **Rejected**: Approver has rejected the request  
- **Invalidated**: Approver is no longer required (removed from workflow)

### 5. Approver Groups
Support for group-based approvals where any group member can approve:

```csharp
var financeTeam = new PAFApproverGroup { GroupID = 1 };
financeTeam.SetApprovers(new[] { "user1@company.com", "user2@company.com" });
financeTeam.SetActiveStatus(true);

workflow.RequestApproval(financeTeam, "Finance Team");
```

## Common Use Cases

PhilApprovalFlow is perfect for:

### Financial Approvals
- Invoice processing and approval
- Purchase order authorizations
- Expense report reviews
- Budget allocation requests

### Document Management
- Contract approvals
- Policy document reviews
- Legal document sign-offs
- Marketing material approvals

### Human Resources
- Leave request approvals
- Hiring decision workflows
- Performance review processes
- Training request approvals

### Project Management
- Project proposal approvals
- Milestone sign-offs
- Change request processes
- Resource allocation approvals

## Architecture Overview

PhilApprovalFlow follows clean architecture principles:

```
┌─────────────────────────┐
│      Your Entity        │ ← Implements IApprovalFlow<T>
├─────────────────────────┤
│      Flow Extensions    │ ← GetApprovalFlow() extension
├─────────────────────────┤
│      Engine Core        │ ← PhilApprovalFlowEngine<T>
├─────────────────────────┤
│   Interfaces & Models   │ ← Core contracts and data
└─────────────────────────┘
```

### Key Components

- **Flow.cs**: Extension methods that provide the entry point
- **PhilApprovalFlowEngine**: Core engine handling all workflow logic
- **Interfaces**: Well-defined contracts (ICanSetUser, ICanAction)
- **Models**: Rich data models (PAFTransition, PAFApproverGroup)
- **Extensions**: Query helpers (IsApproved, IsInTransitions, etc.)

## Integration Examples

### With ASP.NET Core Web API

```csharp
[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveInvoice(int id, [FromBody] ApprovalRequest request)
    {
        var invoice = await _repository.GetByIdAsync(id);
        
        var workflow = invoice.GetApprovalFlow();
        workflow.SetUserName(User.Identity.Name)
               .Approve(request.Comments);
        
        await _repository.SaveAsync();
        return Ok();
    }
}
```

### With Entity Framework

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceTransition> InvoiceTransitions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>()
            .HasMany(i => i.Transitions)
            .WithOne()
            .HasForeignKey("InvoiceId");
    }
}
```

### With Background Services

```csharp
public class NotificationService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var pendingApprovals = await GetPendingApprovalsAsync();
            
            foreach (var approval in pendingApprovals)
            {
                var workflow = approval.Entity.GetApprovalFlow();
                workflow.LoadNotification(approval.ApproverId);
                
                var notifications = workflow.GetPAFNotifications();
                await SendNotificationsAsync(notifications);
                
                workflow.ClearNotifications();
            }
            
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}
```

## Performance Considerations

- **Lazy Loading**: Transitions are loaded on-demand
- **Query Optimization**: Use `Include()` for eager loading when needed
- **Notification Caching**: Built-in notification cache prevents duplicates
- **Minimal Memory Footprint**: Efficient data structures and algorithms

## Security and Validation

While PhilApprovalFlow handles workflow logic, you should implement:

- **Authentication**: Verify user identity before setting user context
- **Authorization**: Check if users can perform specific actions
- **Input Validation**: Validate all inputs before calling workflow methods
- **Audit Logging**: Log all workflow actions for compliance

## Getting Started Checklist

1. ✅ Install the NuGet package
2. ✅ Create your transition model inheriting from `PAFTransition`
3. ✅ Implement `IApprovalFlow<T>` in your entity
4. ✅ Configure database relationships (if using EF)
5. ✅ Start using the fluent API in your business logic
6. ✅ Implement notification handling
7. ✅ Add proper error handling and validation

## Next Steps

Ready to dive deeper? Here's where to go next:

1. **[Getting Started Guide](getting-started.md)**: Step-by-step tutorial with code examples
2. **[Implementation Guide](implementation.md)**: Detailed implementation patterns
3. **[API Reference](../api/index.md)**: Complete API documentation

## Support and Community

- **Documentation**: Comprehensive guides and API reference
- **GitHub Issues**: Report bugs and request features
- **Sample Projects**: Working examples and templates
- **Community**: Active community support

---

*PhilApprovalFlow - Making approval workflows simple, flexible, and powerful.*