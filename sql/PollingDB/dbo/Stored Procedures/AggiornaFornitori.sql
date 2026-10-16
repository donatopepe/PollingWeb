


CREATE Procedure [dbo].[AggiornaFornitori]

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
set @PROCname = 'AggiornaFornitori'
print @pi_istituto

insert into [eProcurement].[dbo].DimFornitori(IstID,CFor,RagSoc,Indirizzo,Cap,Citta,Prov,CodFisc,PIva)
select Istituto,Cfor,Ragione_Sociale, Indirizzo, Cap,Citta,Prov,Codfis,Piva
from TargetSC00G100  a 
where 
a.Istituto=@pi_istituto and
not exists 
(select * from [eProcurement].[dbo].DimFornitori b where a.Istituto=b.IstID and a.Cfor=b.CFor)


update [eProcurement].[dbo].DimFornitori set 
RagSoc=b.Ragione_Sociale, Indirizzo=b.Indirizzo, Cap=b.Cap, Citta=b.Citta, Prov=b.Prov,
CodFisc=b.Codfis, PIva=b.Piva
from [eProcurement].[dbo].DimFornitori a, TargetSC00G100 b
where a.IstID=b.Istituto and a.CFor=b.Cfor and
b.Istituto=@pi_istituto



fine:

--restituisco il valore
set @po_valoreuscita=''


--success
return 0



GRANT EXEC ON AggiornaFornitori TO PUBLIC

