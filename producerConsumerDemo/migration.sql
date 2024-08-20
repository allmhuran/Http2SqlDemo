:on error exit
use scratch;
set xact_abort on;
begin tran;
go

if (schema_id('PCD') is not null) set noexec on;
go
create schema PCD authorization dbo;
go
set noexec off;
go

drop table if exists PCD.Data;
create table PCD.Data(i int primary key, s varchar(32), dt datetime);
commit;



