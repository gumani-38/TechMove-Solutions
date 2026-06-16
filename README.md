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
[Github Project Link](https://github.com/gumani-38/TechMove-Solutions)

# Presentation Video 
The presentation video will be in the project named TechMove_Presenation.mp4

# Technical Report 

1. Delivering frequent, incremental changes is the main goal of continuous integration and deployment (CI/CD), which allows you to receive regular feedback on your product or service. Faster and more frequent delivery, however, shouldn't lower the product's quality.Testing has long been a component of software development processes and is crucial to guaranteeing software quality.It is not feasible to conduct a complete set of manual tests at least once a day. For this reason, a crucial component of any CI/CD pipeline is automated testing.Among the many advantages of automated testing are Every code modification is examined to make sure it functions as intended and hasn't added any new bugs.Feedback is provided more quickly than when the same tests are carried out by hand  (JetBrains, 2000).
2. For instance, you applications is successfuly running within your local laptop or pc but when you attempt to shipp it in a diffirent environment suchs as servers in cloud it fails this is a identified as "It work on my machine problem". Usually, environment drift is the cause of the issue rather than your code. The production server just doesn't have the same OS configurations, environment variables, and library versions as your laptop.Docker is your code's pod. It ensures that your program runs flawlessly on a MacBook, Windows computer, or Linux server by packaging it with all the dependencies it requires. Thereofore, solving the it work on my machine problem (Unalmis, 2023).

# Reference List 
JetBrains. n.d. Automated testing in CI/CD. [online] JetBrains. Available at: <https://www.jetbrains.com/teamcity/ci-cd-guide/automated-testing/> [Accessed 16 June 2026].

Unalmis, S. 2023. Let’s stop the “it works on my machine” drama together: A Docker guide. [online] Medium. Available at: <https://medium.com/@senaunalmis/lets-stop-the-it-works-on-my-machine-drama-together-a-docker-guide-3642a32df746> [Accessed 16 June 2026].

