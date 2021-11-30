# JetBlue.ESE.Net
ESE (Extensible Storage Engine / JetBlue) Json Document Store for .Net 6.0 / C#

uses:

serilog [error logging],

json serialization (currently newtonsoft but will change to more optimized: Utf8Json or System.Text.Json source generators)

autofac ioc modules

---

based on Esent.Interop c# managed wrapper of ESENT:
https://github.com/microsoft/Extensible-Storage-Engine

ESE(/Joint Engine Technology) is with us inside windows os since Windows NT 3.51, now you can use it easly 
inside windows 10-11

indexed sequential access data storage technology. ESE runtime 
has been a part of Windows since version 2000, empowering such products 
as Exchange, Active Directory and Desktop Search. Windows Mail .
Windows 8 is not an exception. ESE is used  and is available as a native API 
for all Windows Store apps.

ESE is an embedded / ISAM-based database engine, that provides rudimentary table and indexed access. 


it's very similar to the idea of sqlite
instead of full blown sql server db ...
and alot faster the sql server



it's A Non-SQL Database Engine

The Extensible Storage Engine (ESE) is one of those rare codebases having proven to have a more than 25 year serviceable lifetime.  First shipping in Windows NT 3.51 and shortly thereafter in Exchange 4.0, and rewritten twice in the 90s, and heavily updated over the subsequent two decades after that, it remains a core Microsoft asset to this day.

- It's running on 100s of thousands of machines and millions of disks for the Office 365 Mailbox Storage Backend servers
- It's also running on large SMP systems with TB of memory for large Active Directory deployments
- Every single Windows Client computer has several database instances running in low memory modes. In over 1 billion Windows 10 devices today, ESE has been in use in Windows client SKUs since Windows XP

ESE enables applications to store data to, and retrieve data from tables using indexed or sequential cursor navigation.  It supports denormalized schemas including wide tables with numerous sparse columns, multi-valued columns, and sparse and rich indexes.  ESE enables applications to enjoy a consistent data state using transacted data update and retrieval.  A crash recovery mechanism is provided so that data consistency is maintained even in the event of a system crash.  ESE provides ACID (Atomic Consistent Isolated Durable) transactions over data and schema by way of a write-ahead log and a snapshot isolation model.

- A summary of features and the JET API documentation are up on [Microsoft's official documentation site](https://docs.microsoft.com/en-us/windows/win32/extensible-storage-engine/extensible-storage-engine)
- A more extensive list of ESE database features [are documented in Wikipedia entry](https://en.wikipedia.org/wiki/Extensible_Storage_Engine)

