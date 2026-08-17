\# Refactor Notes — OrderController



\## 1. Giant controller action



\*\*Smell:\*\* `POST /api/orders` contains almost all application behavior in one method.



\*\*Consequence:\*\* The method is difficult to understand, test, review, and safely modify.



\*\*Fix:\*\* Move business orchestration into an `OrderService` and keep the controller responsible for HTTP concerns only.



\---



\## 2. Direct EF Core access from the controller



\*\*Smell:\*\* `OrderController` directly uses `ShopDbContext`.



\*\*Consequence:\*\* The controller is tightly coupled to persistence and database implementation details.



\*\*Fix:\*\* Introduce a repository/data-access abstraction and inject it through DI.



\---



\## 3. Synchronous EF calls inside an async action



\*\*Smell:\*\* Calls such as `FirstOrDefault()`, `ToList()`, and `SaveChanges()` are used inside `async Task<object> Post(...)`.



\*\*Consequence:\*\* Synchronous database operations can block request threads and reduce scalability.



\*\*Fix:\*\* Use EF Core asynchronous APIs such as `FirstOrDefaultAsync`, `ToListAsync`, and `SaveChangesAsync`, passing a cancellation token.



\---



\## 4. Empty catch blocks



\*\*Smell:\*\* There are four `catch { }` blocks that silently discard exceptions.



\*\*Consequence:\*\* Database failures can be hidden, leaving the application in an unknown state and making production diagnosis extremely difficult.



\*\*Fix:\*\* Remove unnecessary try/catch blocks or catch only expected exceptions, log them, and rethrow when the caller needs to handle them.



\---



\## 5. Off-by-one loop bug



\*\*Smell:\*\* The item loop uses `i <= req.Items.Count`.



\*\*Consequence:\*\* The loop accesses one element beyond the end of the collection and can throw `IndexOutOfRangeException`.



\*\*Fix:\*\* Use `i < req.Items.Count`, or preferably iterate directly over the collection with `foreach`.



\---



\## 6. Possible null dereference



\*\*Smell:\*\* A product lookup can return null, but `p.IsDiscontinued` is accessed immediately.



\*\*Consequence:\*\* An unknown product ID can cause a `NullReferenceException`.



\*\*Fix:\*\* Explicitly validate the product lookup and return an appropriate domain/application error when the product does not exist.



\---



\## 7. Duplicated validation



\*\*Smell:\*\* `req.Items` is checked twice for null/empty conditions.



\*\*Consequence:\*\* Duplicate validation makes the method noisy and increases the chance that the two checks diverge.



\*\*Fix:\*\* Centralize request validation in a validator or validation component.



\---



\## 8. Duplicated stock validation



\*\*Smell:\*\* Stock is checked during item processing and then checked again after stock has already been modified.



\*\*Consequence:\*\* The second check is confusing and does not provide a reliable transaction boundary.



\*\*Fix:\*\* Put stock validation and modification into the service/repository transaction and enforce the invariant in one place.



\---



\## 9. Magic strings



\*\*Smell:\*\* Values such as `"BLOCKED"`, `"GOLD"`, `"SILVER"`, `"BASIC"`, and `"PENDING"` are hard-coded throughout the method.



\*\*Consequence:\*\* Typos become runtime bugs and changing business states requires editing controller code.



\*\*Fix:\*\* Introduce enums, constants, or domain types for statuses and membership levels.



\---



\## 10. Magic numbers



\*\*Smell:\*\* Discount thresholds, discount percentages, shipping prices, tax rates, and loyalty calculations are hard-coded.



\*\*Consequence:\*\* Business rules are difficult to discover, configure, test, and change.



\*\*Fix:\*\* Move business rules into dedicated services/policies or configuration-backed components.



\---



\## 11. Too many responsibilities



\*\*Smell:\*\* The controller handles orders, inventory, discounts, tax, shipping, loyalty points, promotions, membership upgrades, and notifications.



\*\*Consequence:\*\* A change in any one business area requires modifying the controller and increases regression risk.



\*\*Fix:\*\* Separate responsibilities into domain/application services.



\---



\## 12. Repeated database saves



\*\*Smell:\*\* `SaveChanges()` is called repeatedly throughout the request.



\*\*Consequence:\*\* This creates unnecessary database round trips and makes the operation vulnerable to partial updates if a later operation fails.



\*\*Fix:\*\* Coordinate the operation through a transaction/unit-of-work boundary and save at the appropriate point.



\---



\## 13. No explicit transaction around the order workflow



\*\*Smell:\*\* Order creation, order lines, loyalty points, promotion usage, membership changes, and notification creation are persisted through separate saves.



\*\*Consequence:\*\* A failure in the middle can leave the database partially updated.



\*\*Fix:\*\* Use an application transaction around the atomic order operation.



\---



\## 14. Untyped HTTP response



\*\*Smell:\*\* The action returns `Task<object>` and constructs anonymous response objects.



\*\*Consequence:\*\* The API contract is unclear to callers and tooling, and compile-time guarantees are reduced.



\*\*Fix:\*\* Introduce explicit request/response DTOs and return typed `ActionResult<T>` or appropriate typed HTTP results.



\---



\## 15. HTTP concerns mixed with business rules



\*\*Smell:\*\* The controller directly decides when to return `BadRequest`, `NotFound`, or anonymous success objects while also performing business operations.



\*\*Consequence:\*\* Business logic becomes coupled to ASP.NET Core and is harder to unit test independently.



\*\*Fix:\*\* Have the service return domain/application results and let the controller translate those results into HTTP responses.



\---



\## 16. No cancellation support



\*\*Smell:\*\* The async action does not accept or propagate a `CancellationToken`.



\*\*Consequence:\*\* Database work can continue after the client has disconnected.



\*\*Fix:\*\* Accept a cancellation token in the controller and pass it through every asynchronous service and EF Core operation.



\---



\## 17. Controller-level response construction



\*\*Smell:\*\* The controller manually constructs `responseLines` and the final anonymous response.



\*\*Consequence:\*\* API representation logic is mixed into the application workflow.



\*\*Fix:\*\* Use dedicated response DTOs and map application results to those DTOs at the API boundary.



\---



\## Refactoring strategy



1\. Introduce request and response DTOs.

2\. Create an `IOrderService` abstraction.

3\. Move order business logic into `OrderService`.

4\. Introduce repository/data-access abstractions.

5\. Register dependencies through ASP.NET Core DI.

6\. Convert all EF operations to asynchronous APIs.

7\. Flow `CancellationToken` through controller → service → repository → EF.

8\. Remove empty exception handlers and add narrow, logged exception handling where genuinely required.

9\. Replace magic strings/numbers with domain types or named configuration.

10\. Add unit tests for the service/business rules.

11\. Add an integration test using `WebApplicationFactory`.

12\. Verify the refactored API preserves the intended behavior while eliminating the identified bugs.

