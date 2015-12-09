/*
drop table GeoZones
drop table Orders
drop table Users
drop table Logs
*/

if object_id('Logs', 'U') is null
begin
    create table Logs
    (
        [Id] int not null identity(1, 1)
            constraint PK_Logs primary key clustered,
        [Message] varchar(max) null,
        [MessageTemplate] nvarchar(max) null,
        [Level] varchar(128) null,
        [TimeStamp] datetime not null,
        [Exception] varchar(max) null,
        [Properties] xml null,
        [EventType] varchar(255) null
    )
end
go

if object_id('Users', 'U') is null
begin
    create table Users
    (
        [Id] int not null identity(1, 1)
            constraint PK_Users primary key clustered,
        [Name] varchar(255) not null
    )
end
go

if not exists (select * from Users)
begin
    insert into Users
        ([Name])
    values
        ('Acid Burn'),
        ('Phantom Phreak'),
        ('Lord Nikon'),
        ('Mr. The Plague'),
        ('Cereal Killer'),
        ('Zero Cool'),
        ('Crash Override')
end
go
        

if object_id('Orders', 'U') is null
begin
    create table Orders
    (
        [Id] int not null identity(1, 1)
            constraint PK_Orders primary key clustered,
        [UserId] int not null
            constraint FK_Orders_Users foreign key references Users(Id),
        [Date] datetime not null,
        [Product] varchar(255) not null,
        [Quantity] int not null,
        [Price] decimal(19, 4) not null
    )
end
go

if not exists (select * from Orders)
begin
    insert into Orders
        ([UserId], [Date], [Product], [Quantity], [Price])
    select
        u.Id,
        getdate(),
        f.Name,
        1 + abs(checksum(newId())) % 100,
        1000 + cast(abs(checksum(newId())) % 1000000 as decimal(19, 4)) /100
    from Users u
    cross join
    (
        select [Name] = 'Apricot' union all
        select 'Avocado' union all
        select 'Banana' union all
        select 'Bilberry' union all
        select 'Blackberry' union all
        select 'Blackcurrant' union all
        select 'Blueberry' union all
        select 'Currant' union all
        select 'Cherry' union all
        select 'Cloudberry' union all
        select 'Coconut' union all
        select 'Cranberry' union all
        select 'Damson' union all
        select 'Gooseberry' union all
        select 'Grape' union all
        select 'Huckleberry' union all
        select 'Lemon' union all
        select 'Lime' union all
        select 'Melon' union all
        select 'Orange' union all
        select 'Pumpkin' union all
        select 'Raspberry' union all
        select 'Squash' union all
        select 'Tomato'
    ) f
end
go

if object_id('GeoZones', 'U') is null
begin
    create table GeoZones
    (
        [Id] int not null identity(1, 1)
            constraint PK_GeoZones primary key clustered,
        [Name] varchar(255) not null,
        [MinIPNumber] bigint not null,
        [MaxIPNumber] bigint not null
    )
end
go

if not exists (select * from GeoZones)
begin
    declare @max bigint = dbo.IPToInt('255.255.255.255')
    declare @size bigint = @max / 10

    insert into GeoZones
        ([Name], [MinIPNumber], [MaxIPNumber])
    select
        c.Name,
        cast(@size * c.[Rank] as bigint),
        cast(@size * (c.[Rank] + 1) as bigint) + (case c.[Rank] when 9 then 0 else -1 end)
    from
    (
        -- Top Ten Hacking Countries from Bloomberg
        select [Rank] = 0, [Name] = 'China' union all
        select 1, 'United States' union all
        select 2, 'Turkey' union all
        select 3, 'Russia' union all
        select 4, 'Taiwan' union all
        select 5, 'Brazil' union all
        select 6, 'Romania' union all
        select 7, 'India' union all
        select 8, 'Italy' union all
        select 9, 'Hungary'
    ) c
end
go

if object_id('IntToIP', 'FN') is not null
    drop function dbo.IntToIP
go

create function dbo.IntToIP(@number bigint)
returns varchar(255)
begin
 
    return
        convert(varchar(3), (@number/16777216) & 255) + '.' +
        convert(varchar(3), (@number/65536) & 255) + '.' +
        convert(varchar(3), (@number/256) & 255) + '.' +
        convert(varchar(3),  @number & 255)
end
go

if object_id('IPToInt', 'FN') is not null
    drop function dbo.IPToInt
go

create function dbo.IPToInt(@address varchar(255))
returns bigint
begin

    return
        convert(bigint, parseName(@address,1)) +
        convert(bigint, parseName(@address,2)) * 256 +
        convert(bigint, parseName(@address,3)) * 65536 +
        convert(bigint, parseName(@address,4)) * 16777216
 
end
go