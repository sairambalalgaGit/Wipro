var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseEndpoints(endpoints =>
{
    // Complex product route
    endpoints.MapControllerRoute(
        name: "productDetails",
        pattern: "Products/{category}/{id:int}",
        defaults: new { controller = "Products", action = "Details" });

    // Filter with custom constraints
    endpoints.MapControllerRoute(
        name: "productFilter",
        pattern: "Products/Filter/{category}/{priceRange}",
        defaults: new { controller = "Products", action = "Filter" });

    // Checkout dynamic routing
    endpoints.MapControllerRoute(
        name: "checkout",
        pattern: "Checkout",
        defaults: new { controller = "Cart", action = "Checkout" });

    // Default route
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});



app.Run();
