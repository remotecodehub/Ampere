using Ampere.Composition.Extensions;
await WebApplication
    .CreateBuilder(args)
    .RunAmpereAsync<Program, Ampere.Components.App>();
