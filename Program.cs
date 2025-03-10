using System.Reflection.Metadata.Ecma335;
using HoneyRaesAPI.Models;

List<Customer> customers = new List<Customer> 
{
    new Customer { Id = 1, Name = "Toren", Address= "123 New St, Nashville, TN" },
    new Customer { Id = 2, Name = "Sirena", Address = "456 Park Rd, Nashville, TN"},
    new Customer { Id =3, Name = "Brian", Address = "789 Main St, Nashville, TN"} 
 };
List<Employee> employees = new List<Employee> 
{
    new Employee { Id = 1, Name = "Matt", Specialty = "Debugging" },
    new Employee { Id = 2, Name= "Casey", Specialty= "Practical Jokes" }
};
List<ServiceTicket> serviceTickets = new List<ServiceTicket>
{
    new ServiceTicket { Id = 1, CustomerId = 1, EmployeeId = 1, Description = "Too many bugs in the console.", Emergency = true, DateCompleted = null },
    new ServiceTicket { Id = 2, CustomerId = 3, EmployeeId = 2, Description = "Too much work", Emergency = false, DateCompleted = DateTime.Today },  
    new ServiceTicket { Id = 3, CustomerId = 3, EmployeeId = 1, Description = "Buggin.", Emergency = false, DateCompleted = null },
    new ServiceTicket { Id = 4, CustomerId = 2, EmployeeId = 2, Description = "Overworked.", Emergency = true, DateCompleted = DateTime.Today },
    new ServiceTicket { Id = 5, CustomerId = 1, EmployeeId = null, Description = "Too many bugs in the console.", Emergency = false, DateCompleted = null }
 };



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/servicetickets", () =>
{
    return serviceTickets;
});

app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    serviceTicket.Customer =customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);
    return Results.Ok(serviceTicket);
});

app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(employee);
});

app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    } 
    customer.ServiceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    return Results.Ok(customer);
});

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

app.MapDelete("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.Find(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    } 
    serviceTickets.Remove(serviceTicket);
    return Results.Ok();
    });

app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    //the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});

app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today;
});

app.MapGet("/servicetickets/emergencies", () =>
{
    List<ServiceTicket> incompleteEmergencies = serviceTickets.Where(st => st.Emergency == true && st.DateCompleted == null).ToList();
    if (incompleteEmergencies == null)
    { 
        return Results.NotFound(); 
    }
    else
    {
        return Results.Ok(incompleteEmergencies);
    }
    
});

app.MapGet("/servicetickets/unassigned", () =>
{
    List<ServiceTicket> noEmployee = serviceTickets.Where(st => st.EmployeeId == null).ToList();
    // (!noEmployee.Any())
    if (noEmployee == null) 
    {
        return Results.NotFound();
    }
    else{
        return Results.Ok(noEmployee);
    }
});

app.Run();
