using Ampere.Components;
using Ampere.Composition.Extensions;
await WebApplication
    .CreateBuilder(args)
    .RunAmpereAsync<App>();

