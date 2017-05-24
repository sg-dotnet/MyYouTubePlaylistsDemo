# MyYouTubePlaylistsDemo
This application is used to show a simple ASP .NET Core 2.0 web application with authentication using Google and connecting to YouTube Data API v3 to retrieve the playlists of logged-in user.

This app is developed on **macOS 10.12 (aka macOS Sierra)** using **Visual Studio Code v.1.12.2**.

## Setup

### Install .NET Core 2.0 Preview 1
Please visit https://www.microsoft.com/net/core/preview#macos to download the official installer.

### Scaffolding Web Application
 1. Create a working directory for your project and navigate to it;
 2. Run the following command on **Terminal**;
    ```
    dotnet new mvc --auth Individual
    ```
 3. When the CLI command completes, the following output will be displayed;
    ```
    The template "ASP.NET Core Web App (Model-View-Controller)" was created successfully.
    This template contains technologies from parties other than Microsoft, see https://aka.ms/template-3pn for details.
    ```
 4. Please update C# extension to the latest one 1.9.0.
 5. You should see the following line in the .csproj file.
    ```
    <TargetFramework>netcoreapp2.0</TargetFramework>
    ```
 6. Open the project folder in **Visual Studio Code** and select the **Startup.cs** file. VS Code will prompt to restore the needed project dependencities and add build/debug dependencies. Please choose **Restore** and **Yes** to both of them, respectively.
    - Do not try to set the Debug Configuration yourself otherwise the project can't be built because of missing the tasks.json file. =)
 
### First Attempt To Build
When we first build the project, the following exception will be thrown.
```
Unhandled Exception: System.InvalidOperationException: **Unable to configure HTTPS endpoint**.

For information on configuring HTTPS see https://go.microsoft.com/fwlink/?linkid=848054. 

---> System.InvalidOperationException: No certificate found for endpoint 'LocalhostHttps'.
```

### Adding Certificate
So, based on the link given above, we will need to set up HTTPS by using self-signed certificate for development in ASP.NET Core on macOS. This can be done using the **OpenSSL** as shown below.

```
openssl req -new -x509 -newkey rsa:2048 -keyout localhostMyYouTubePlaylists.key -out localhostMyYouTubePlaylists.cer -days 365 -subj /CN=localhost    
*You need to provide PEM pass phrase after this*

openssl pkcs12 -export -out certificateMyYouTubePlaylists.pfx -inkey localhostMyYouTubePlaylists.key -in localhostMyYouTubePlaylists.cer    
*You need to provide the exact same pass phrase that you entered in the previous command for the key.*    
*You also need to enter an Export Password.*
```

We then need to add the certificate to our keychain and change its trust settings so that it is trusted for HTTPS during development.

```
security import certificateMyYouTubePlaylists.pfx -k ~/Library/Keychains/login.keychain-db
*You need to provide the Export Password you entered earlier.*

security add-trusted-cert localhostMyYouTubePlaylists.cer
*You need to enter your macOS username and password here since you are making changes to the trust settings.*
```

Finally, since we store the certificate in our keychain, which is the equivalent of the CurrentUser/My store on Windows, we need to change set the following for the HTTPS certificate in **appSettings.Development.json**.
```
{
    "Certificates": {
        "HTTPS": {
            ...
            "StoreLocation": "CurrentUser"
        }
        ...
    }
}
```

Now, when we visit the HTTPS version of our web application, it will prompt the following.
```
**dotnet wants to sign using key "privateKey" in your keychain.

The authenticity of "dotnet" cannot be verified. Do you want to allow acccess to this item?
```

Please press **"Always Allow"**.

Finally, let's move on to the urlRewrite.config and please make sure the rules given in template taking care of both cases, i.e. localhost and 127.0.0.1, as shown below.
```
<?xml version="1.0" encoding="utf-8"?>
<rewrite>
  <rules>
    <rule name="Redirect to https">
      <match url="(.*)" />
      <conditions>
        <add input="{HTTPS}" pattern="Off" />
        <add input="{HTTP_HOST}" negate="true" pattern="(localhost|127.0.0.1)" />
      </conditions>
      <action type="Redirect" redirectType="Temporary" url="https://{HTTP_HOST}/{R:1}" />
    </rule>
    <rule name="Redirect to https (localhost)">
      <match url="(.*)" />
      <conditions>
        <add input="{HTTPS}" pattern="Off" />
        <add input="{HTTP_HOST}" pattern="(localhost|127.0.0.1):55555" />
      </conditions>
      <action type="Redirect" redirectType="Temporary" url="https://localhost:43434/{R:1}" />
    </rule>
  </rules>
</rewrite>
```

### Second Attempt To Build
After doing all these, we can finally see our webpage on web browser.

Now, let's click on the **"Sign in"** button on the top-right corner of the page. We will see a page saying that we need to update our database first to run the migration scripts to create tables for storing user info.

### Migration for IdentityServiceDbContext
We can apply pending migrations from a command prompt at our project directory using the command below.
```
dotnet ef database update
```

Right after running this, an exception will be thrown.
```
System.InvalidOperationException: Unable to configure HTTPS endpoint. For information on configuring HTTPS see https://go.microsoft.com/fwlink/?linkid=848054. 

---> System.Collections.Generic.KeyNotFoundException: No certificate named 'HTTPS' found in configuration for the current environment (Production).
```

Previously, we can still set the environment of migration in Entity Framework Core .NET Command Line Tools which is defaulted to "Development". However, this option is no longer available now.

Thus, this becomes [one of the **known issues** in ASP.NET Core 2.0 and Web Tools](https://github.com/aspnet/Tooling/blob/master/known-issues-vs2017-preview.md). Official recommendation is to start a Developer Command Prompt, set the environment variable ASPNETCORE_ENVIRONMENT=Development and then start VS with this environment variable set.

Alternatively, we can also temporarily move the entire **""Certificates""** section from the **appsettings.Development.json** to **appsettings.json**. Then when we execute the EF migration command again, the script will be run successfully.

Now if we launch our web app again, we can successfully reach the **Sign in** page.

### Configuring Google Authentication
We will leave the local authentication at a side first. Let's look at how we can introduce Google Login as an alternative authentication service.

Before we can proceed, we need to create a new **Project** in [Google API Console](https://console.developers.google.com/apis).

In the API Manager of the newly created Project, we need to first enabled the Google+ API. After that, we will get its **Credential** by clicking on the button **"Create credentials"**. The credential that we are looking for is one for **Web Servers** which allows us to access **User Data** so that we can successfully log the users in to our web app using Google Login.

We will be promoted to enter **Authorized Redirect URIs**. Here, we need to enter our current development site URL with signin-google appended into the Authorized redirect URIs field. For example, https://localhost:xxxxx/signin-google.

We will then be prompted to download the **Client ID** and **Client Secret**.

## Managing Secrets
The **Secret Manager tool** offers us a way to store sensitive data in our local development machine. To use it, we first need to install this command line tool via Nuget to our project.

```
dotnet add package Microsoft.Extensions.SecretManager.Tools -v 2.0.0-preview1-final
```

We just need to execute the following commands in our project working directory in order to store the Google secrets above:

```
dotnet user-secrets set Authentication:Google:ClientID <client-id>
dotnet user-secrets set Authentication:Google:ClientSecret <client-secret>
```

For each successful command, the Terminal will print the following line.

```
Successfully saved ... to the secret store.
```

### Adding Google Login
Back to our working directory. There is an area known as **IdentityService**. We will configure external authentication handlers in its file **IdentityServiceStartup.cs**.

```
services.AddGoogleAuthentication(g => {
  g.ClientId = context.Configuration["Authentication:Google:ClientID"];
  g.ClientSecret = context.Configuration["Authentication:Google:ClientSecret"];
  g.SignInScheme = IdentityCookieOptions.ExternalScheme;
});
```

Please take note that if **``g.SignInScheme = IdentityCookieOptions.ExternalScheme;``** is not added, then when we call **``_signInManager.GetExternalLoginInfoAsync()``** in **ExternalLoginConfirmation** method, GetExternalLoginInfoAsync() will always returns us null and prevents the login to happen successfully.

### Calling YouTube Data APIs
Now, since we can authenticate the user using Google Login in our web app, how about we retrieve some user info from Google? To do so, the service we are trying in the demo is YouTube.

First of all, we need to enable the **YouTube Data API** in the Google API Console. After that, we need to get another API key which is restricted to IP Address.

Then, we need to use scope to request for permissions from the users. YouTube Data API has their scopes defined at https://developers.google.com/youtube/v3/guides/auth/server-side-web-apps. In this demo, we will only use the API to get the list of playlists from the logged-in user. So the scope we are going to use is `https://www.googleapis.com/auth/youtube.readonly`.

```
services.AddGoogleAuthentication(g => {
  g.ClientId = context.Configuration["Authentication:Google:ClientID"];
  g.ClientSecret = context.Configuration["Authentication:Google:ClientSecret"];
  g.SignInScheme = IdentityCookieOptions.ExternalScheme;
  g.DisplayName = "YouTube (Google)";
  g.Scope.Add("https://www.googleapis.com/auth/youtube.readonly");
  g.SaveTokens = true;
});
```

The **``g.SaveToken``** is important. Otherwise, we will not be able to get the token in the authentication step. Without token, we then cannot retrieve the user's playlists.

So, to retrieve the token, we have the following statement in **ExternalLogin** method.

```
  TempData["Token"] = info.AuthenticationTokens.Single(t => t.Name == "access_token").Value;
```

With this token, we then can retrieve logged-in user's YouTube data, such as his/her playlists.

```
using (var client = new HttpClient())
{
    client.DefaultRequestHeaders.Authorization =
        AuthenticationHeaderValue.Parse("Bearer " + youtubeToken);

    using (var response = await client.GetAsync(
        $"https://www.googleapis.com/youtube/v3/playlists?part=snippet,status&key=...&mine=true&maxResults=10"))
    {
        try
        {
            string stringResponse = await response.Content.ReadAsStringAsync();
            return View(JsonConvert.DeserializeObject<YouTubePlaylistResponse>(stringResponse));
        }
        catch (HttpRequestException){...}
    }
}
```

### References
 - [[Microsoft Docs] Setting up HTTPS for development in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/https)
 - [Known issues for .NET Core 2.0, ASP.NET Core 2.0, and ASP.NET and Web Tools in Visual Studio 2017 "Preview"](https://github.com/aspnet/Tooling/blob/master/known-issues-vs2017-preview.md)
 - [Configuring Google authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins)
 - [UserSecrets: obsolete APIs removed in 2.0](https://github.com/aspnet/Announcements/issues/223)
 - [Using OAuth 2.0 for Web Server Applications](https://developers.google.com/youtube/v3/guides/auth/server-side-web-apps)
