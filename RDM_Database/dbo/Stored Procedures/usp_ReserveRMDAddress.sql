
create procedure [dbo].[usp_ReserveRMDAddress]
@rdmAddress int output
as 
BEGIN
insert into RDMAddress(DateActivated)
Values(GetDate())
set @rdmAddress = ident_current('RDMAddress');
END