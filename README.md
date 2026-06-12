# Gumani_Moila_EAPD7111w_POE

## Set up and Installation
1. Ensure you are using .Net 10 version upwards.
2. [Install Microsoft Sql express 2025 (https://www.microsoft.com/en-us/sql-server/sql-server-downloads)]and [SQL Server management Studio] (https://learn.microsoft.com/en-us/ssms/install/install)
3. Ensure the SQL express server is running and the generated server name  so that you can you the connection string in SQL server management Studio.
4. In the Application go to appsetting.json file look for connection strings, you will find "Server=localhost\\SQLEXPRESS;Database=TechMoveDb;Trusted_Connection=True;TrustServerCertificate=True;" if your server name is diffirent please update hear.

## Database Set up
After ensuring your SQL express server and SQL server management are working and connection is established successfully.

### Open the Package manager console  terminal within the project in Visual Studio
If the packages of entity framework core are not installed run this first
```
dotnet tool install --global dotnet-ef
```
This will create the Database named TechMoveDb or whatever name you have given it in the connection string setting json file, if it does not exists  and the tables including their relationships
```
Add-Migration InitialCreate
Update-Database
```

After successfully creating. now you can run the project.


## Nunits Test
<img width="1919" height="1014" alt="image" src="https://github.com/user-attachments/assets/7705bc9b-95c6-48c4-831c-91c1f82a444d" />


# Manual SSMS(SQL SERVER MANAGEMENT STUDIO) Database Migration Script
Look for TechMove_Database_Script with in the main project [TechMove_Database_Script.sql](https://github.com/user-attachments/files/27955378/TechMove_Database_Script.sql)

# GITHUB Link
[Github Project Link](https://github.com/gumani-38/Gumani_Moila_EAPD7111w_POE)

# Presentation Video 
The presentation video will be in the project named TechMove_Presenation.mp4
