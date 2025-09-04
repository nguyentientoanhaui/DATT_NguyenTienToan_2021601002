-- Check ProductImages for product ID 959
SELECT 
    p.Id as ProductId,
    p.Name as ProductName,
    pi.Id as ImageId,
    pi.ImageName,
    pi.IsDefault,
    pi.ColorId
FROM Products p
LEFT JOIN ProductImages pi ON p.Id = pi.ProductId
WHERE p.Id = 959
ORDER BY pi.IsDefault DESC, pi.Id;

-- Check all products that have images
SELECT 
    p.Id as ProductId,
    p.Name as ProductName,
    COUNT(pi.Id) as ImageCount
FROM Products p
LEFT JOIN ProductImages pi ON p.Id = pi.ProductId
GROUP BY p.Id, p.Name
HAVING COUNT(pi.Id) > 0
ORDER BY p.Id;

-- Check all image records in ProductImages table
SELECT TOP 10
    pi.Id,
    pi.ProductId,
    pi.ImageName,
    pi.IsDefault,
    pi.ColorId,
    p.Name as ProductName
FROM ProductImages pi
LEFT JOIN Products p ON pi.ProductId = p.Id
ORDER BY pi.ProductId, pi.IsDefault DESC;

