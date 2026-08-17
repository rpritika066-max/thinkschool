\# Initial AI Prompt — Deliberately Bad OrderController



Create a deliberately bad legacy `OrderController.cs` for an ASP.NET Core 10 Web API.



Requirements:



\- Approximately 300 lines of code.

\- Use one giant `POST /api/orders` action.

\- Put business logic, EF Core data access, validation, calculations, persistence, and HTTP response construction directly inside the controller.

\- Inject `ShopDbContext` directly into the controller.

\- Do not use service or repository layers.

\- Use synchronous EF Core calls such as `FirstOrDefault()`, `ToList()`, and `SaveChanges()` inside an `async` action.

\- Return `object` from the action instead of a strongly typed response.

\- Use anonymous objects for HTTP responses.

\- Include four empty `catch { }` blocks that swallow exceptions.

\- Include an off-by-one bug in an item-processing loop.

\- Include a possible null dereference when looking up a product.

\- Include duplicated validation.

\- Include duplicated stock validation.

\- Include repeated `SaveChanges()` calls throughout the workflow.

\- Include hard-coded magic strings for customer status, membership levels, and order status.

\- Include hard-coded magic numbers for discounts, shipping, tax, and loyalty calculations.

\- Mix order creation, inventory updates, discounts, tax, shipping, loyalty points, promotions, membership upgrades, and notifications in the same controller action.

\- Do not accept or propagate a `CancellationToken`.

\- Include inline response shaping.

\- Make the code look like realistic legacy production code rather than an intentionally silly example.

\- Include subtle bugs that a refactoring exercise should uncover.

\- Do not refactor the code.

\- Do not add tests.

\- Save the resulting file as `OrderController.cs`.



The purpose is to simulate receiving a poorly maintained legacy controller from a colleague and then performing a production-style refactor.

