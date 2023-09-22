# Dev notes

`dotnet watch` to start an app with the hot reload

Right now I'm trying to implement a weird "streaming" way of sending images to the client.
By streaming I mean that instead of sending image files one by one with an `image/jpeg` MIME type
I want to have a long living Response with a byte stream. So all images go there - plus metadata to be
able to say when one image ends.
Right now I've implemented some simple writer on a Backend, but I was unable to handle it on a front.
It does work with a CURL request - it logs that response comes in chunks.
Also it's fine in FF
But Chrome waits for Response to end.
However it seems that the Streams API is the right way to handle it on the clent.
For the future reference see https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream/getReader

15.09.2023
While working on the `GS-5` I've decided to put some efforts into figuring out how the heck the whole app starts,
what is the `WebApplicationBuilder`, why sometimes we use `UseStartup` to configure services and other details related to it.

19.09.2023
Just a recap for using Configuration Settings in controllers, services etc
Injecting IConfiguration is not approved, instead use

```csharp
public class MySettings
{
    public string FirstValue {get; set;}
    public string SecondValue {get; set;}
}

IConfigurationSection configSection = builder.Configuration.GetSection("mySettingsConfigName");

builder.Services.Configure<MySettings>(configSection)
```

Then you can inject it as `IOptions<MySettings>`

What the code above does, afaik - the `GetSection` will just create the path to the config, by using the name provided.
Later it will be combined with all the configs magic the framework has and voila - we get or config, considering that 
we have it in, say, `appsettings.json`:

```json
"mySettingsConfigName": {
    "FirstValue": "Hello"
    "SecondValue": "World"
    }
```

`builder.Services.Configure` has an overload that accepts 'The name of the options instance' as one of the parameters and I don't know
what is it supposed to mean. When I put just a random string there, the configuraion was empty.

Another interesting thing I've found is about `WebApplicationOptions.ApplicationName`
It cannot be some random name you'd like, because of the following code from the `HostingHostBuilderExtensions.ApplyDefaultAppConfiguration`

```csharp
if (env.IsDevelopment() && env.ApplicationName is { Length: > 0 })
{
    var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
    if (appAssembly is not null)
    {
        appConfigBuilder.AddUserSecrets(appAssembly, optional: true, reloadOnChange: reloadOnChange);
    }
}
```

As we can see at `Assembly.Load(new AssemblyName(env.ApplicationName))` on dev environment the framework will try 
to load an assembly based on provided `ApplicationName` - thus it should be the same as the assmbly name.
I really don't understand the purpose of this property.

TODO: Figure out how to change the environments. Read https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-7.0