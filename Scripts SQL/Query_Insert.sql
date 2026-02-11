
--________________________________ INSERT ROL ________________________________
insert into rol([description],isActive) values
('Admin',1),
('Employee',1),
('Inspector',1)

go


--________________________________ INSERT USER DEFAULT ________________________________

insert into Users(name,email,phone,idRol,[password],photo,isActive) values
('codeStudent','codeStudent@example.com','909090',1,'123',null,1)

go

--________________________________ INSERT CATEGORIES ________________________________

INSERT INTO Category([description],isActive) values
('Computers',1),
('Laptops',1),
('Keyboards',1),
('Monitors',1),
('Microphones',1)

go

--________________________________ INSERT TYPEDOCUMENTSALE ________________________________

insert into TypeDocumentSale([description],isActive) values
('Ticket',1),
('invoice',1)

go
--________________________________ INSERT CORRELATIVE NUMBER ________________________________

--000001
insert into CorrelativeNumber(lastNumber,quantityDigits,management,dateUpdate) values
(0,6,'Sale',getdate())


go
--________________________________ INSERT MENUS ________________________________

--*menu parent

insert into Menu([description],icon,isActive) values
('Admin','mdi mdi-view-dashboard-outline',1),
('Inventory','mdi mdi-package-variant-closed',1),
('Sales','mdi mdi-shopping',1),
('Reports','mdi mdi-chart-bar',1)


--*menu child - Admin
insert into Menu([description],idMenuParent,controller,pageAction,isActive) values
('DashBoard',1,'Admin','DashBoard',1),
('Users',1,'Admin','Users',1)


--*menu child - Inventory
insert into Menu([description],idMenuParent,controller,pageAction,isActive) values
('Categories',2,'Inventory','Categories',1),
('Products',2,'Inventory','Products',1)

--*menu child - Sales
insert into Menu([description],idMenuParent,controller,pageAction,isActive) values
('New sale',3,'Sales','NewSale',1),
('Sales history',3,'Sales','SalesHistory',1)

--*menu hijos - Reports
insert into Menu([description],idMenuParent,controller,pageAction,isActive) values
('Sales report',4,'Reports','SalesReport',1)


UPDATE Menu SET idMenuParent = idMenu where idMenuParent is null


go
--________________________________ INSERT MENU ROLE ________________________________


--*Admin
INSERT INTO RolMenu(idRol,idMenu,isActive) values
(1,5,1),
(1,6,1),
(1,7,1),
(1,8,1),
(1,9,1),
(1,10,1),
(1,11,1)

--*Employee
INSERT INTO RolMenu(idRol,idMenu,isActive) values
(2,9,1),
(2,10,1)

--*Inspector
INSERT INTO RolMenu(idRol,idMenu,isActive) values
(3,7,1),
(3,8,1),
(3,9,1),
(3,10,1)