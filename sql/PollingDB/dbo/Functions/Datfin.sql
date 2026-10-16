-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
create FUNCTION [dbo].[Datfin]
(
	@pi_Datrif	datetime
)
RETURNS datetime
AS
BEGIN
	-- Declare the return variable here
	DECLARE @vl_Datfin datetime
	set @vl_Datfin = CONVERT(datetime,
	RIGHT('0'+ CONVERT(varchar(2), day(@pi_Datrif)),2) + '/' +
	RIGHT('0'+ CONVERT(varchar(2), month(@pi_Datrif)),2) + '/' +
	CONVERT(varchar(4), YEAR(@pi_Datrif)),103)

	

	-- Return the result of the function
	RETURN @vl_Datfin

END

