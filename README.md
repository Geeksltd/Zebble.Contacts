[logo]: https://raw.githubusercontent.com/Geeksltd/Zebble.Contacts/master/icon.png "Zebble.Contacts"


## Zebble.Contacts

![logo]

Contacts gives you a unified API to query the device contacts.


[![NuGet](https://img.shields.io/nuget/v/Zebble.Contacts.svg?label=NuGet)](https://www.nuget.org/packages/Zebble.Contacts/)

> Modern smartphones have a central repository of contacts managed by the operating system. That is used for normal telephone functionality, but it's also available to apps via an API on each platform.

<br>


### Setup
* Available on NuGet: [https://www.nuget.org/packages/Zebble.Contacts/](https://www.nuget.org/packages/Zebble.Contacts/)
* Install in your platform client projects.
* Available for iOS, Android and UWP.
<br>


### Api Usage

Use Zebble.Device.Contacts from any project to gain access to APIs.

##### Get all device contacts:
```csharp
var contacts = await Device.Contacts.GetAll();
```
##### Searching for specific contacts:
```csharp
var contacts = await Device.Contacts.Search("Jack Smith");
```

<br>


### Methods
| Method       | Return Type  | Parameters                          | Android | iOS | Windows |
| :----------- | :----------- | :-----------                        | :------ | :-- | :------ |
| GetAll         | Task<IEnumerable<Contact&gt;&gt;| errorAction -> OnError | x       | x   | x       |
| Search         | Task<IEnumerable<Contact&gt;&gt;| nameKeywords -> string<br> errorAction -> OnError| x       | x   | x       |
