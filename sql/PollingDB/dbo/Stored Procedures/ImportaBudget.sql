

CREATE Procedure [dbo].[ImportaBudget]

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
set @PROCname = 'ImportaBudget'

--declare @pi_istituto int
--set @pi_istituto=63

declare @vl_codcdr nvarchar(4)
declare @vl_materiale nvarchar(10)
declare @vl_budgetril float
declare @vl_budgetres float
declare @vl_codfondo nvarchar(50)
declare @vl_datarif	datetime
declare @vl_sapid int
declare @vl_idbdg int

UPDATE eProcurement.dbo.DimBudget SET [IsActive] = 0 
WHERE ([IsActive] = 1) AND IstID=@pi_istituto

DECLARE cur_budget CURSOR FOR
	SELECT    
		CodCDR,
		a.Materiale,
		BudgetRilasci,
		BudgetResiduo,
		CodFondo,
		Datarif
	FROM	
		eProcurement.dbo.DimBudgetTemp a,
		eProcurement.dbo.DimSottoCatSAP b
	WHERE	
		a.Materiale=b.Materiale and
		Istituto=@pi_istituto

	OPEN cur_budget
	FETCH NEXT FROM cur_budget INTO 
		@vl_codcdr,	
		@vl_materiale,
		@vl_budgetril,
		@vl_budgetres,
		@vl_codfondo,
		@vl_datarif
	WHILE @@FETCH_STATUS = 0
	begin
		SELECT @vl_sapid= IDCatSAP FROM eProcurement.dbo.DimSottoCatSAP
		where Materiale=@vl_materiale

		print @vl_materiale 
		SELECT @vl_idbdg=IDBdg FROM eProcurement.dbo.DimBudget 
		WHERE 
			DataRif=@vl_datarif and 
			SapID=@vl_sapid		and
			CodCdR=@vl_codcdr	and			
			CodFondo=@vl_codfondo and
			IstID=@pi_istituto 

		if (@@ROWCOUNT=0)
		begin
			--se non esiste record
			INSERT INTO eProcurement.dbo.DimBudget 
			([IstID],[CodFondo],[SapID],[CodCdR],[Materiale],BudgetAttuale,[BudgetResiduo],[DataRif])
			VALUES 
			(@pi_istituto,@vl_codfondo,@vl_sapid,@vl_codcdr,@vl_materiale,@vl_budgetril,@vl_budgetres,@vl_datarif)
		end
		else
		begin
			--aggiorno budget
			UPDATE eProcurement.dbo.DimBudget SET [BudgetResiduo] = @vl_budgetres,BudgetAttuale=@vl_budgetril, [IsActive] = 1
			WHERE [IDBdg] = @vl_idbdg  and IstID=@pi_istituto
		end

		FETCH NEXT FROM cur_budget INTO 
			@vl_codcdr,	
			@vl_materiale,
			@vl_budgetril,
			@vl_budgetres,
			@vl_codfondo,
			@vl_datarif
	end

CLOSE cur_budget
DEALLOCATE cur_budget





fine:

--restituisco il valore
set @po_valoreuscita=''


--success
return 0



GRANT EXEC ON ImportaBudget TO PUBLIC

