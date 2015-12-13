select
    [UserName] = u.Name,
    [ZoneName] = z.Name,
    [PurchaseCount] = count(*),
    [PurchasePercent] = cast(cast(count(*) / t.Count * 100 as int) as varchar(255)) + '%'
from
(
    select
        [OrderId] = l.Properties.value('(/properties/property[@key="OrderId"])[1]', 'varchar(255)'),
        [NumberAddress] = dbo.IPToInt(l.Properties.value('(/properties/property[@key="UserAddress"])[1]', 'varchar(255)'))
    from Logs l
    where l.EventType = '"PurchaseCommited"'
) p
join Orders o on o.Id = p.OrderId
join Users u on u.Id = o.UserId
cross apply (select [Count] = cast(count(*) as decimal(19, 4)) from Orders ot where ot.UserId = u.Id) t
join GeoZones z on z.MinIPNumber <= p.NumberAddress and z.MaxIPNumber >= p.NumberAddress
group by u.Name, t.Count, z.Name
order by u.Name, count(*) desc