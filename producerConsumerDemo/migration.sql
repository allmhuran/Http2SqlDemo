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
go

if (type_id('pcd.data') is not null) set noexec on;
go
create type pcd.data as table(i int primary key, s varchar(32), dt datetime);
go
set noexec off;
go

create or alter procedure [PCD].[InsertData](@data pcd.data readonly) as
begin
   set nocount on;
   insert pcd.data select * from @data;
end
go

create or alter procedure [PCD].[MergeData](@data pcd.data readonly) as 
begin
   set nocount on;
   
   merge    pcd.data t
   using    @data    s on s.i = t.i

   when not matched then
   insert (i, s, dt) values (s.i, s.s, s.dt)

   when matched then
   update
   set   t.i = s.i,
         t.s = s.s,
         t.dt = s.dt;
end;
go

commit;



