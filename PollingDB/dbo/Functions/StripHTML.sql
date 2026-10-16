
 

create FUNCTION [dbo].[StripHTML] (@HTMLText NVARCHAR(MAX))

RETURNS NVARCHAR(MAX)

AS

BEGIN

      DECLARE @Start INT

      DECLARE @End INT

      DECLARE @Length INT

      SET @Start = CHARINDEX('<',@HTMLText)

      SET @End = CHARINDEX('>',@HTMLText,CHARINDEX('<',@HTMLText))

      SET @Length = (@End - @Start) + 1

      WHILE (@Start > 0) AND (@End > 0) AND (@Length > 0)

      BEGIN

            SET @HTMLText = STUFF(@HTMLText,@Start,@Length,' ')

            SET @Start = CHARINDEX('<',@HTMLText)

            SET @End = CHARINDEX('>',@HTMLText,CHARINDEX('<',@HTMLText))

            SET @Length = (@End - @Start) + 1

      END

 

      declare @normTxt nvarchar(max)

      set @normTxt = Replace(@HTMLText, '''', ' ')

      set @normTxt = Replace(@normTxt, ',', ' ')

      set @normTxt = Replace(@normTxt, '`', ' ')

      set @normTxt = Replace(@normTxt, '’', ' ')

      set @normTxt = Replace(@normTxt, '-', ' ')

      set @normTxt = Replace(@normTxt, 'à', 'a')

      set @normTxt = Replace(@normTxt, 'è', 'e')

      set @normTxt = Replace(@normTxt, 'ì', 'i')

      set @normTxt = Replace(@normTxt, 'ò', 'o')

      set @normTxt = Replace(@normTxt, 'ù', 'u')

     

      return @normTxt

END

 
