# PhilApprovalFlow Library

**Approval Flow Library for .NET Projects**

PhilApprovalFlow is a powerful and flexible approval workflow library designed for .NET applications. It provides a fluent API to easily implement approval processes for any entity in your application.

## 🚀 Key Features

- **Fluent API Design** - Intuitive and readable method chaining for building approval workflows
- **Entity Attachment** - Seamlessly attaches to any entity implementing the IApprovalFlow interface
- **Multi-Framework Support** - Works with .NET Framework 4.8+ and .NET 8+
- **Built on .NET Standard 2.0** - Ensures broad compatibility across different .NET implementations
- **Individual & Group Approvers** - Support for both single approvers and approver groups
- **Rich Transition Management** - Track approval states, comments, and timestamps
- **Built-in Notification System** - Generate notifications for approvers and requesters
- **Metadata Support** - Attach custom metadata to workflows and entities
- **Check-in Functionality** - Track when approvers view pending requests
- **Decision States** - Comprehensive decision tracking (Approved, Rejected, Invalidated, Awaiting)

## 📦 Installation

Install PhilApprovalFlow via NuGet Package Manager:

```bash
Install-Package PhilApprovalFlow -Version 1.3.0
```

Or via .NET CLI:

```bash
dotnet add package PhilApprovalFlow --version 1.3.0
```

## 🔥 Quick Start

```csharp
// 1. Create an entity that implements IApprovalFlow
public class Invoice : IApprovalFlow<InvoiceTransition>
{
    public ICollection<InvoiceTransition> Transitions { get; set; }
    public Guid InvoiceId { get; set; }
    
    public object GetID() => InvoiceId;
    public string GetShortDescription() => $"Invoice #{InvoiceId}";
    public string GetLongDescription() => $"Invoice for {Amount:C} - {Description}";
}

// 2. Create your transition type
public class InvoiceTransition : PAFTransition, IPAFTransition { }

// 3. Use the fluent API
var invoice = new Invoice();
var workflow = invoice.GetApprovalFlow();

// Request approval from a manager
workflow.SetUserName("employee@company.com")
        .RequestApproval("manager@company.com", "Manager", "Please approve this invoice");

// Manager approves the request
workflow.SetUserName("manager@company.com")
        .Approve("Approved - looks good!");

// Check if fully approved
bool isApproved = invoice.Transitions.IsApproved();
```

## 📚 Documentation Sections

### [Getting Started](articles/getting-started.md)
Learn how to set up and use PhilApprovalFlow in your project.

### [Implementation Guide](articles/implementation.md)
Detailed guide on implementing approval workflows in your entities.

### [API Reference](api/index.md)
Complete API documentation for all classes and methods.

## 🔗 Useful Links

- **GitHub Repository**: [https://github.com/vish-j/PhilApprovalFlow](https://github.com/vish-j/PhilApprovalFlow)
- **NuGet Package**: [PhilApprovalFlow on NuGet](https://www.nuget.org/packages/PhilApprovalFlow/)
- **Issues & Support**: [GitHub Issues](https://github.com/vish-j/PhilApprovalFlow/issues)

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](https://github.com/vish-j/PhilApprovalFlow/blob/main/CONTRIBUTING.md) for details.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/vish-j/PhilApprovalFlow/blob/main/LICENSE) file for details.

---

*Built with ❤️ for the .NET Community*