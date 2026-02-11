
create database POINTOFSALE

go

use POINTOFSALE

go

create table Menu(
[idMenu] int primary key identity(1,1),
[description] varchar(30),
[idMenuParent] int references Menu(idMenu),
[icon] varchar(30),
[controller] varchar(30),
[pageAction] varchar(30),
[isActive] bit,
[registrationDate] datetime default getdate()
)

go


create table Rol(
[idRol] int primary key identity(1,1),
[description] varchar(30),
[isActive] bit,
[registrationDate] datetime default getdate()
)
 go
 
 create table RolMenu(
 [idRolMenu] int primary key identity(1,1),
 [idRol] int references Rol(idRol),
 [idMenu] int references Menu(idMenu),
 [isActive] bit,
 [registrationDate] datetime default getdate()
 )

 go


create table Users(
[idUsers] int primary key identity(1,1),
[name] varchar(50),
[email] varchar(50),
[phone] varchar(50),
[idRol] int references Rol(idRol),
[password] varchar(100),
[photo] varbinary(max),
[isActive] bit,
[registrationDate] datetime default getdate()
)

go

create table Category(
[idCategory] int primary key identity(1,1),
[description] varchar(50),
[isActive] bit,
[registrationDate] datetime default getdate()
)

go

create table Product(
[idProduct] int primary key identity(1,1),
[barCode] varchar(50),
[brand] varchar(50),
[description] varchar(100),
[idCategory] int references Category(idCategory),
[quantity] int,
[price] decimal(10,2),
[photo] varbinary(max),
[isActive] bit,
[registrationDate] datetime default getdate()
)

go

create table CorrelativeNumber(
[idCorrelativeNumber] int primary key identity(1,1),
[lastNumber] int,
[quantityDigits] int,
[management] varchar(100),
[dateUpdate] datetime
)

go


create table TypeDocumentSale(
[idTypeDocumentSale] int primary key identity(1,1),
[description] varchar(50),
[isActive] bit,
[registrationDate] datetime default getdate()
)

go

create table Sale(
[idSale] int primary key identity(1,1),
[saleNumber] varchar(6),
[idTypeDocumentSale] int references TypeDocumentSale(idTypeDocumentSale),
[idUsers] int references Users(idUsers),
[customerDocument] varchar(10),
[clientName] varchar(20),
[Subtotal] decimal(10,2),
[totalTaxes] decimal(10,2),
[total] decimal(10,2),
[registrationDate] datetime default getdate()
)

go


create table DetailSale(
[idDetailSale] int primary key identity(1,1),
[idSale] int references Sale(idSale),
[idProduct] int,
[brandProduct] varchar(100),
[descriptionProduct] varchar(100),
[categoryProducty] varchar(100),
[quantity] int,
[price] decimal(10,2),
[total] decimal(10,2)
)

go

create table Negocio(
idNegocio int primary key,
urlLogo varchar(500),
nombreLogo varchar(100),
numeroDocumento varchar(50),
nombre varchar(50),
correo varchar(50),
direccion varchar(50),
telefono varchar(50),
porcentajeImpuesto decimal(10,2),
simboloMoneda varchar(5)
)
