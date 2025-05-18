# PhilApprovalFlow API Reference

PhilApprovalFlow provides a comprehensive Fluent API for implementing approval workflows in .NET applications. This reference covers all public interfaces, classes, and extension methods.

## Core Interfaces

### IApprovalFlow&lt;T&gt;
The main interface that entities must implement to support approval workflows.

```csharp
public interface IApprovalFlow<T> where T : IPAFTransition
{
    ICollection<T> Transitions { get; set; }
    object GetID();
    string GetShortDescription();
    string GetLongDescription();
}
```

**Properties:**
- `Transitions`: Collection of workflow transitions
  
**Methods:**
- `GetID()`: Returns the entity's unique identifier
- `GetShortDescription()`: Returns a brief description for notifications
- `GetLongDescription()`: Returns a detailed description for audit logs

### ICanSetUser
Entry point interface for setting user context.

```csharp
public interface ICanSetUser
{
    ICanAction SetUserName(string username);
    ICanAction ResetTransitions(string comments = null);
}
```

**Methods:**
- `SetUserName(string)`: Sets the current user context
- `ResetTransitions(string)`: Resets all transitions to AwaitingDecision state

### ICanAction
Primary interface for performing workflow actions.

```csharp
public interface ICanAction
{
    // Metadata operations
    ICanAction SetMetadata(string key, string value);
    string GetMetadata(string key);
    void SetEntityMetaData();
    
    // Approval operations
    ICanAction RequestApproval(string approver, string role, string comments = null);
    ICanAction RequestApproval(IPAFApproverGroup group, string role, string comments = null);
    ICanAction CheckIn();
    ICanAction Approve(string comments = null);
    ICanAction Reject(string comments = null);
    ICanAction Invalidate(string approver, string comments = null);
    
    // Notification operations
    ICanAction LoadNotification(string approver, string[] usersToCC = null, string[] mailsToCC = null);
    ICanAction LoadNotification(IPAFApproverGroup group, string[] usersToCC = null, string[] mailsToCC = null);
    IEnumerable<IPAFNotification> GetPAFNotifications();
    void ClearNotifications();
}
```

### IPAFTransition
Interface defining the structure of approval transitions.

```csharp
public interface IPAFTransition
{
    Guid TransitionID { get; set; }
    int Order { get; set; }
    string ApproverID { get; set; }
    DecisionType ApproverDecision { get; set; }
    string ApproverRole { get; set; }
    DateTime? ApproverCheckInDate { get; set; }
    DateTime? AcknowledgementDate { get; set; }
    string ApproverComments { get; set; }
    string RequesterID { get; set; }
    DateTime RequestedDate { get; set; }
    string RequesterComments { get; set; }
    bool IsCheckedIn { get; }
    PAFApproverGroup ApproverGroup { get; set; }
    
    void Initialize(int order, string requester, string approver, string role, string comments);
    void Initialize(int order, string requester, IPAFApproverGroup group, string role, string comments);
}
```

### IPAFApproverGroup
Interface for defining approver groups.

```csharp
public interface IPAFApproverGroup : IEnumerable<string>
{
    long GroupID { get; set; }
    bool IsActive();
}
```

### IPAFNotification
Interface representing workflow notifications.

```csharp
public interface IPAFNotification
{
    string From { get; set; }
    string To { get; set; }
    string Comments { get; set; }
    DecisionType DecisionType { get; set; }
    long? GroupID { get; set; }
    string[] UsersToCC { get; set; }
    string[] MailsToCC { get; set; }
}
```

## Core Classes

### PAFTransition
Abstract base class for approval transitions.

```csharp
public abstract class PAFTransition : IPAFTransition
{
    public Guid TransitionID { get; set; }
    public int Order { get; set; }
    public string ApproverID { get; set; }
    public string ApproverRole { get; set; }
    public PAFApproverGroup ApproverGroup { get; set; }
    public DateTime? ApproverCheckInDate { get; set; }
    public DateTime? AcknowledgementDate { get; set; }
    public DecisionType ApproverDecision { get; set; }
    public string ApproverComments { get; set; }
    public string RequesterID { get; set; }
    public DateTime RequestedDate { get; set; }
    public string RequesterComments { get; set; }
    public bool IsCheckedIn => ApproverCheckInDate != null;
    
    public void Initialize(int order, string requester, string approver, string role, string comments);
    public void Initialize(int order, string requester, IPAFApproverGroup group, string role, string comments);
}
```

**Constructors:**
- `PAFTransition()`: Default constructor
- `PAFTransition(PAFTransition t)`: Copy constructor

### PAFApproverGroup
Implementation of approver groups.

```csharp
public class PAFApproverGroup : IPAFApproverGroup
{
    public long GroupID { get; set; }
    
    public void SetApprovers(IEnumerable<string> approvers);
    public IEnumerable<string> GetApprovers();
    public bool IsActive();
    public void SetActiveStatus(bool isActive);
    public IEnumerator<string> GetEnumerator();
}
```

**Methods:**
- `SetApprovers(IEnumerable<string>)`: Sets the list of approvers in the group
- `GetApprovers()`: Returns the list of approvers
- `IsActive()`: Checks if the group is currently active
- `SetActiveStatus(bool)`: Activates or deactivates the group

### PAFNotification
Implementation of workflow notifications.

```csharp
public class PAFNotification : IPAFNotification
{
    public string From { get; set; }
    public string To { get; set; }
    public string Comments { get; set; }
    public DecisionType DecisionType { get; set; }
    public long? GroupID { get; set; }
    public string[] UsersToCC { get; set; }
    public string[] MailsToCC { get; set; }
}
```

## Enumerations

### DecisionType
Defines the possible states of an approval transition.

```csharp
public enum DecisionType
{
    AwaitingDecision = 0,   // Initial state, waiting for decision
    Approved = 1,           // Approver has approved
    Rejected = 2,           // Approver has rejected
    Invalidated = 3         // Approver no longer required
}
```

## Extension Methods

### Flow Extensions
Main entry point for the approval workflow.

```csharp
public static class Flow
{
    public static ICanSetUser GetApprovalFlow<T>(this IApprovalFlow<T> f) where T : IPAFTransition, new();
}
```

**Usage:**
```csharp
var workflow = entity.GetApprovalFlow();
```

### ApprovalEngineExtensions
Query and utility methods for working with transitions.

```csharp
public static class ApprovalEngineExtensions
{
    // User in workflow checks
    public static bool IsInTransitions(this IEnumerable<IPAFTransition> transitions, string username, bool includeInvalidated = false);
    
    // Approval state checks
    public static bool IsApprovedEnabled(this IEnumerable<IPAFTransition> transitions, string username);
    public static bool IsRejectEnabled(this IEnumerable<IPAFTransition> transitions, string username);
    public static bool IsTakenDecision(this IEnumerable<IPAFTransition> transitions, string username);
    public static bool IsCheckedIn(this IEnumerable<IPAFTransition> transitions, string username);
    
    // Workflow state checks
    public static bool IsApproved(this IEnumerable<IPAFTransition> transitions);
    public static bool IsAnyApproved(this IEnumerable<IPAFTransition> transitions);
    public static bool IsAnyDecisionPending(this IEnumerable<IPAFTransition> transitions);
}
```

**Method Descriptions:**

#### User Workflow Methods
- `IsInTransitions(string, bool)`: Checks if a user has a transition in the workflow
- `IsApprovedEnabled(string)`: Determines if a user can approve their transition
- `IsRejectEnabled(string)`: Determines if a user can reject their transition  
- `IsTakenDecision(string)`: Checks if a user has made a decision
- `IsCheckedIn(string)`: Checks if a user has checked in to their transition

#### Workflow State Methods
- `IsApproved()`: Returns true if all non-invalidated transitions are approved
- `IsAnyApproved()`: Returns true if any transition is approved
- `IsAnyDecisionPending()`: Returns true if any transition is awaiting decision

## Attributes

### PAFMetadataAttribute
Allows attaching metadata to entity classes.

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class PAFMetadataAttribute : Attribute
{
    public string Key { get; set; }
    public string Value { get; set; }
}
```

**Usage:**
```csharp
[PAFMetadata(Key = "EntityType", Value = "Invoice")]
[PAFMetadata(Key = "Module", Value = "Finance")]
public class Invoice : IApprovalFlow<InvoiceTransition>
{
    // Implementation
}
```

## Usage Examples

### Basic Approval Flow
```csharp
// Request approval
entity.GetApprovalFlow()
      .SetUserName("requester@company.com")
      .RequestApproval("approver@company.com", "Manager", "Please review");

// Approve request
entity.GetApprovalFlow()
      .SetUserName("approver@company.com")
      .Approve("Looks good!");

// Check status
bool isApproved = entity.Transitions.IsApproved();
```

### Group Approval
```csharp
var group = new PAFApproverGroup { GroupID = 1 };
group.SetApprovers(new[] { "user1@company.com", "user2@company.com" });
group.SetActiveStatus(true);

entity.GetApprovalFlow()
      .SetUserName("requester@company.com")
      .RequestApproval(group, "Finance Team");
```

### Metadata Operations
```csharp
entity.GetApprovalFlow()
      .SetUserName("user@company.com")
      .SetMetadata("Priority", "High")
      .SetMetadata("Department", "IT")
      .SetEntityMetaData();

string priority = workflow.GetMetadata("Priority");
```

### Notification Handling
```csharp
entity.GetApprovalFlow()
      .SetUserName("requester@company.com")
      .LoadNotification("approver@company.com", 
                       usersToCC: new[] { "manager@company.com" },
                       mailsToCC: new[] { "notifications@company.com" });

var notifications = workflow.GetPAFNotifications();
// Process notifications
workflow.ClearNotifications();
```

## Error Handling

The API throws the following exceptions:

- **ArgumentNullException**: When required parameters are null
- **ArgumentException**: When parameters are invalid (empty strings, etc.)
- **InvalidOperationException**: When operations are performed in invalid states
- **KeyNotFoundException**: When accessing non-existent metadata keys

## Thread Safety

PhilApprovalFlow is designed for concurrent access scenarios. However, you should ensure proper synchronization when:

- Modifying transitions from multiple threads
- Accessing/modifying metadata concurrently
- Persisting workflow state to databases

## Performance Notes

- Extension methods are optimized for LINQ-to-Objects scenarios
- Notification cache prevents duplicate notifications
- Use eager loading (`Include()`) when querying entities with transitions
- Consider using projection queries for read-only operations

---

For implementation examples and best practices, see the [Getting Started Guide](../articles/getting-started.md) and [Implementation Guide](../articles/implementation.md).