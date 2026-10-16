


CREATE Procedure [dbo].[AzzeraTargetSC00G100]

(
@pi_istituto smallint,
--
@po_valoreuscita varchar(30) output
)



AS

declare
--gestione errori
@DBrowcount int,
@DBerror int,
@DBreturn_value int,
@PROCname varchar(50),
@vl_giacenza float,


@DBfetch_status int

--none procedura
set @PROCname = 'AzzeraTargetSC00G100'

delete TargetSC00G100 
where Istituto=@pi_istituto




fine:

--restituisco il valore
set @po_valoreuscita=''


--success
return 0



GRANT EXEC ON AzzeraTargetSC00G100 TO PUBLIC

