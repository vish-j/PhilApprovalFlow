# PhilApprovalFlow

[![NuGet Version](https://img.shields.io/nuget/v/PhilApprovalFlow.svg)](https://www.nuget.org/packages/PhilApprovalFlow/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PhilApprovalFlow.svg)](https://www.nuget.org/packages/PhilApprovalFlow/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**A powerful and flexible approval workflow library for .NET applications**

PhilApprovalFlow is a comprehensive approval workflow framework that simplifies the implementation of complex approval processes in .NET applications. Built with a fluent API design, it seamlessly integrates with any entity in your application to provide robust workflow management.

## ✨ Key Features

- **🔥 Fluent API Design** - Intuitive method chaining for readable workflow code
- **🔗 Entity Attachment** - Works with any class implementing `IApprovalFlow<T>`
- **🌐 Multi-Framework Support** - Compatible with .NET 8+ and .NET Framework 4.8+
- **📦 .NET Standard 2.0** - Broad compatibility across .NET implementations
- **👥 Individual & Group Approvers** - Support for both single users and approver groups
- **📊 Rich State Management** - Track approvals, rejections, and invalidations
- **🔔 Built-in Notifications** - Generate notifications for all workflow participants
- **📝 Metadata Support** - Attach custom data to workflows and entities
- **✅ Check-in Functionality** - Track when approvers view pending requests

## 🚀 Quick Start

### Installation

Install via NuGet Package Manager:

```bash
Install-Package PhilApprovalFlow -Version 1.2.14
```

Or via .NET CLI:

```bash
dotnet add package PhilApprovalFlow --version 1.2.14
```

### Basic Usage

```csharp
using PhilApprovalFlow;
using PhilApprovalFlow.Models;

// 1. Create your transition model
public class InvoiceTransition : PAFTransition, IPAFTransition { }

// 2. Implement IApprovalFlow in your entity
public class Invoice : IApprovalFlow<InvoiceTransition>
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; }
    public decimal Amount { get; set; }
    
    // Required by IApprovalFlow
    public ICollection<InvoiceTransition> Transitions { get; set; }
    
    public Invoice()
    {
        Id = Guid.NewGuid();
        Transitions = new HashSet<InvoiceTransition>();
    }
    
    // Required methods
    public object GetID() => Id;
    public string GetShortDescription() => $"Invoice {InvoiceNumber} - {Amount:C}";
    public string GetLongDescription() => $"Invoice {InvoiceNumber} for {Amount:C}";
}

// 3. Use the fluent API
var invoice = new Invoice();

// Request approval from manager
invoice.GetApprovalFlow()
       .SetUserName("employee@company.com")
       .RequestApproval("manager@company.com", "Manager", "Please approve this invoice");

// Manager approves the request
invoice.GetApprovalFlow()
       .SetUserName("manager@company.com")
       .Approve("Approved - looks good!");

// Check approval status
bool isFullyApproved = invoice.Transitions.IsApproved();
bool hasPendingApprovals = invoice.Transitions.IsAnyDecisionPending();
```

## 📋 Advanced Examples

### Group Approvals

```csharp
// Create approver group
var financeTeam = new PAFApproverGroup { GroupID = 1 };
financeTeam.SetApprovers(new[] 
{
    "finance1@company.com", 
    "finance2@company.com", 
    "finance3@company.com"
});
financeTeam.SetActiveStatus(true);

// Request approval from group
invoice.GetApprovalFlow()
       .SetUserName("employee@company.com")
       .RequestApproval(financeTeam, "Finance Team", "Finance approval required");

// Any group member can approve
invoice.GetApprovalFlow()
       .SetUserName("finance1@company.com")
       .Approve("Budget verified and approved");
```

### Multi-Level Approvals

```csharp
var workflow = invoice.GetApprovalFlow().SetUserName("employee@company.com");

// Set up approval chain based on amount
if (invoice.Amount > 10000)
{
    workflow.RequestApproval("supervisor@company.com", "Supervisor")
           .RequestApproval("manager@company.com", "Manager")  
           .RequestApproval("director@company.com", "Director");
}
else if (invoice.Amount > 1000)
{
    workflow.RequestApproval("manager@company.com", "Manager");
}
else
{
    workflow.RequestApproval("supervisor@company.com", "Supervisor");
}
```

### Notifications

```csharp
// Load notification for approver
invoice.GetApprovalFlow()
       .SetUserName("employee@company.com")
       .LoadNotification("manager@company.com", 
                        usersToCC: new[] { "team@company.com" },
                        mailsToCC: new[] { "notifications@company.com" });

// Get and process notifications
var notifications = workflow.GetPAFNotifications();
foreach (var notification in notifications)
{
    // Send email, create task, etc.
    await SendEmailAsync(notification);
}

// Clear notifications to prevent duplicates
workflow.ClearNotifications();
```

### Metadata Management

```csharp
invoice.GetApprovalFlow()
       .SetUserName("employee@company.com")
       .SetMetadata("Priority", "High")
       .SetMetadata("Department", "Finance")
       .SetMetadata("CostCenter", "CC-001")
       .SetEntityMetaData(); // Sets id, shortDescription, longDescription

// Retrieve metadata
string priority = workflow.GetMetadata("Priority");
string entityId = workflow.GetMetadata("id");
```

## 📚 Documentation

- **[Getting Started Guide](https://vish-j.github.io/PhilApprovalFlow/articles/getting-started.html)** - Complete tutorial with examples
- **[Implementation Guide](https://vish-j.github.io/PhilApprovalFlow/articles/implementation.html)** - Detailed implementation patterns
- **[API Reference](https://vish-j.github.io/PhilApprovalFlow/api/index.html)** - Complete API documentation
- **[Full Documentation](https://vish-j.github.io/PhilApprovalFlow/)** - Comprehensive documentation site

## 💼 Use Cases

PhilApprovalFlow is perfect for:

- **Financial Systems**: Invoice processing, purchase orders, expense reports
- **Document Management**: Contract approvals, policy reviews, content publishing  
- **Human Resources**: Leave requests, hiring workflows, performance reviews
- **Project Management**: Project approvals, milestone sign-offs, change requests
- **General Business**: Any process requiring structured approval workflows

## 🛠️ Framework Compatibility

| Framework | Support Level | Notes |
|-----------|---------------|-------|
| .NET 8+ | ✅ Full Support | Primary target framework |
| .NET 6/7 | ✅ Full Support | Via .NET Standard 2.0 |
| .NET Core 3.1+ | ✅ Full Support | Via .NET Standard 2.0 |
| .NET Framework 4.8+ | ✅ Compatible | Minimum compatibility maintained |

## 🔧 Entity Framework Integration

PhilApprovalFlow works seamlessly with Entity Framework:

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

## 🔄 Workflow State Queries

Use powerful extension methods to query workflow states:

```csharp
// Check if user can approve
bool canApprove = invoice.Transitions.IsApprovedEnabled("user@company.com");

// Check if user is in workflow
bool isInWorkflow = invoice.Transitions.IsInTransitions("user@company.com");

// Check if user has made a decision
bool hasDecided = invoice.Transitions.IsTakenDecision("user@company.com");

// Check overall workflow state
bool isApproved = invoice.Transitions.IsApproved();
bool hasRejections = invoice.Transitions.Any(t => t.ApproverDecision == DecisionType.Rejected);
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CODE_OF_CONDUCT.md) for details.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙋‍♂️ Support

- 📖 **Documentation**: [https://vish-j.github.io/PhilApprovalFlow/](https://vish-j.github.io/PhilApprovalFlow/)
- 🐛 **Issues**: [GitHub Issues](https://github.com/vish-j/PhilApprovalFlow/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/vish-j/PhilApprovalFlow/discussions)

## 🏗️ Built With

- .NET Standard 2.0
- C# 9.0+
- Modern .NET tooling

---

⭐ **If you find PhilApprovalFlow helpful, please consider giving it a star on GitHub!**

*Built with ❤️ for the .NET community*
