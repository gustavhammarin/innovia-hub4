using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Result;
using Innovia.Api.Common.Auth;
using Microsoft.AspNetCore.Identity;
namespace Innovia.Api.Features.Users.CreateUser;
public sealed record Request(string FirstName,string LastName,string Email,string Password);
public sealed record Response(Guid Id,string FirstName,string LastName,string Email);
public static class Endpoint
{ public static void Map(IEndpointRouteBuilder app) => app.MapPost("/", async (Request r, UserManager<ApplicationUser> um, CancellationToken ct) => { var u=new ApplicationUser{Id=Guid.CreateVersion7(),UserName=r.Email.Trim(),Email=r.Email.Trim(),FirstName=r.FirstName.Trim(),LastName=r.LastName.Trim()}; var x=await um.CreateAsync(u,r.Password); if(!x.Succeeded)return Results.BadRequest(x.Errors.Select(e=>e.Description)); await um.AddToRoleAsync(u,Roles.Member); return Results.Ok(new Response(u.Id,u.FirstName,u.LastName,u.Email!)); }); }
