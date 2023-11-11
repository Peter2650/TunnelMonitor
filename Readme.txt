GUID 85070d57-cf76-4366-ab6a-eb4f78cc07d3


dotnet build
dotnet run
dotnet clean
dotnet publish -c Release


Using et fremmed librar:
  using Newtonsoft.Json;
  Kræver:
    Install-Package Newtonsoft.Json -Scope CurrentUser
  
     Den her fejler pga. rettigheder, man skal være administrator 
      Install-Package Newtonsoft.Json

    <ItemGroup> skal tilføjes:
    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <OutputType>WinExe</OutputType>
        <TargetFramework>net7.0-windows</TargetFramework>
        <Nullable>enable</Nullable>
        <UseWPF>true</UseWPF>
      </PropertyGroup>
      <ItemGroup>
        <PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
      </ItemGroup>
    </Project>
