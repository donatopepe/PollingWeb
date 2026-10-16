

CREATE Procedure [dbo].[Flusso_RDA]

(
--@pi_istituto smallint,
@pi_guid	varchar(40),
--
@po_valoreuscita varchar(40) output
)



AS

declare
--gestione errori
@DBrowcount int,
@DBerror int,
@DBreturn_value int,
@PROCname varchar(50),
@tiporecord_DB varchar(3),
@fornitore varchar(10),
@gruppoacquisti varchar(3),

 @GUID uniqueidentifier,
 @NOrdine nvarchar(1000),
 @Istituto int,
 @RigaOrdine int,
 @MesiPeriodo int,
 @MesiTOT int,
 @GiorniPeriodo int,
 @CostoPerGiorno float,
 @FlagFineAnno int,
 @TipoOrd nvarchar(2),
 @OggettoRDA nvarchar(250),
 
 @Frequenza int,
 @Riga_master nvarchar(1000),
 @Riga_slave nvarchar(1000),

 @TipoContabilizzazione nvarchar(1),
 @Materiale nvarchar(150),
 @GruppoMerci nvarchar(9),
 @TestoBreve nvarchar(150),
 @TestoDesc nvarchar(150),
 @CodiceIVA nvarchar(2),
 @QtaC int,

 @CostoTotale float,
 @Per int,
 @DataInizioCompetenzaArt datetime,
 @DataFineCompetenzaArt datetime,
 @DataPrevCons datetime,
 @Cespite nvarchar(12),
 @Fondo nvarchar(10),
 @Categ1 nvarchar(10),
 @DIC datetime,	/* DataInizioCompetenza per riga */
 @DFC datetime,	/* DataFineCompetenza per riga */
 @DFC_END datetime,
 @CostoPeriodo float,	/* Costo per riga */
 @CostoPerMese float,
 @DIC_NEXT datetime,
 @DIC_Print datetime,
 @DIC_NEXT_BK datetime,

 @CdR nvarchar(5),
 @sLineR varchar(MAX),


@DBfetch_status int

--none procedura
set @PROCname = 'Flusso_RDA'
set @tiporecord_DB='RDA'
--print @pi_istituto

declare @tbl_output table(
	riga nvarchar(1000)
)

select @GUID=GUID, 
		@DataInizioCompetenzaArt=DataModifica,
		@DataFineCompetenzaArt=DataModifica,
		@fornitore=right(REPLICATE('0', 10) + COALESCE(cast(CFor as nvarchar),'') , 10),
		@gruppoacquisti=GruppoAcquisti, 
		@OggettoRDA=eProcurement.dbo.StripHTML(Oggetto),
		@NOrdine='4502'+right( REPLICATE('0', 6) + COALESCE(cast(Progressivo as nvarchar),'') , 6),
		@Istituto=Istituto
from eProcurement.dbo.RdA
where GUID=@pi_guid


--inserimento testata
select	@Riga_master =
		'T' + 	'NB02' + '1818' + 
		left(@OggettoRDA+ REPLICATE(' ', 255) , 255)

insert into @tbl_output(riga) values (@Riga_master)	

print 'Riga master ' +@pi_guid
print @Riga_master




set @RigaOrdine = 0
	--ciclo su RDAart
	DECLARE cur_slave CURSOR FOR
	--SELECT DISTINCT         
	--	RDAArt.GruppoMerci,
	--	dbo.StripHTML(coalesce(RDAArt.DescrDett_Ord, @OggettoRDA)) AS TestoBreve, 		
	--	RDAArt.QtaC, 
	--	RDAArt.CostoConcordatoNoIva, 		
	--	RDAArt.DataInizioCompetenza, 
	--	RDAArt.DataFineCompetenza	
	--FROM	RDAArt		
	--WHERE  (RDAArt.RdAID = @GUID)
	SELECT  distinct        
		a.GruppoMerci,
		eProcurement.dbo.StripHTML(coalesce(a.DescrDett_Ord, @OggettoRDA)) AS TestoBreve, 		
		a.QtaC, 
		b.CostoAnno,	--a.CostoConcordatoNoIva, 		
		b.DataInizioAnno, 
		b.DataFineAnno
	FROM	
		eProcurement.dbo.RDAArt	a, 
		eProcurement.dbo.RDAArt_Costo_per_Anno b
	WHERE  
	a.GUIDArt=b.GUIDArt and	
	(a.RdAID = @GUID)

	OPEN cur_slave
	FETCH NEXT FROM cur_slave INTO --@TipoContabilizzazione,
									--@Materiale,
									@GruppoMerci,
									@TestoBreve,
									--@CodiceIVA,
									@QtaC,
									@CostoTotale,
									--@Per,
									@DataInizioCompetenzaArt,
									@DataFineCompetenzaArt
									--@DataPrevCons
									--@Fondo,
									--@Cespite,
									--@Categ1
	WHILE @@FETCH_STATUS = 0
	BEGIN 
		set @RigaOrdine = @RigaOrdine + 10

		set @Riga_slave = 
			'P'+
			right('000' + cast(@RigaOrdine as nvarchar) , 3) +
			'U'+
			' '+
			left(@TestoBreve + REPLICATE(' ', 40),40) +
			right(REPLICATE('0', 11) + COALESCE(cast(@QtaC as nvarchar),'') , 11)+
			'   '+			
			--prezzo 11
			right(REPLICATE('0', 11) + 
			(case when CHARINDEX('.', COALESCE(cast(convert(decimal(10,2), @CostoTotale) as nvarchar),'')) > 0 then
				SUBSTRING ( COALESCE(cast(convert(decimal(10,2), @CostoTotale) as nvarchar),''), 0 , CHARINDEX('.', COALESCE(cast(convert(decimal(10,2), @CostoTotale) as nvarchar),'')) )
			else COALESCE(cast(convert(decimal(10,2), @CostoTotale) as nvarchar),'') end) , 11)+				
			--prezzo 2
			left((case when CHARINDEX('.', COALESCE(cast(convert(decimal(10,2), @CostoTotale) as nvarchar),'')) > 0 then
				SUBSTRING ( COALESCE(cast(convert(decimal(10,2), @CostoTotale) as nvarchar),''), CHARINDEX('.', COALESCE(cast(convert(decimal(10,2), @CostoTotale) as nvarchar),''))+1 , len(COALESCE(cast(convert(decimal(10,2), @CostoTotale) as nvarchar),'')) )
				else '00' end) 
				+ REPLICATE('0', 2) , 2)+
			--data inizio competenza
			right('0000' + COALESCE(cast(DATEPART (yyyy , @DataInizioCompetenzaArt) as nvarchar),'') , 4) + 
			right('00' + COALESCE(cast(DATEPART (m , @DataInizioCompetenzaArt) as nvarchar),'') , 2) + 
			right('00' + COALESCE(cast(DATEPART (d , @DataInizioCompetenzaArt) as nvarchar),'') , 2)+
			--data fine competenza
			right('0000' + COALESCE(cast(DATEPART (yyyy , @DataFineCompetenzaArt) as nvarchar),'') , 4) + 
			right('00' + COALESCE(cast(DATEPART (m , @DataFineCompetenzaArt) as nvarchar),'') , 2) + 
			right('00' + COALESCE(cast(DATEPART (d , @DataFineCompetenzaArt) as nvarchar),'') , 2)+
			@fornitore+
			@GruppoMerci+
			@gruppoacquisti+
			@NOrdine+
			left('' + REPLICATE(' ', 255) , 255)
					
			insert into @tbl_output(riga) values (@Riga_slave)
			print 'Riga slave '  +@pi_guid
			print @Riga_slave
			print 'costo totale ' +convert(varchar,@CostoTotale)
			FETCH NEXT FROM cur_slave INTO 			
									@GruppoMerci,
									@TestoBreve,									
									@QtaC,
									@CostoTotale,
									--@Per,
									@DataInizioCompetenzaArt,
									@DataFineCompetenzaArt
									--@DataPrevCons
	end  --while
	
	CLOSE cur_slave
	DEALLOCATE cur_slave

							
	
	print '@pi_guid ' +@pi_guid 
	print 'NOrdine ' +@NOrdine

	--CANCELLO EVENTUALE RIGHE PRECEDENTE DI GUID NON ANCORA INVIATA E MODIFICATA DALL'UTENTE
	--delete Tbl_Export_SAP_OnLine where substring(Riga,2,10)=@NOrdine and Tiporecord=@tiporecord_DB



fine:

--cancello eventuale rda inserita precedentemente
delete eProcurement.dbo.Tbl_Export_SAP_OnLine 
where NumRda=@NOrdine and Codist=@Istituto


--inserisco righe elaborate
INSERT INTO eProcurement.dbo.Tbl_Export_SAP_OnLine(Riga, Spid,Codist,Tiporecord, NumRda)
		SELECT riga, @@SPID,@Istituto,@tiporecord_DB, @NOrdine
		FROM @tbl_output

--restituisco il valore
set @po_valoreuscita=''


--success
return 0



GRANT EXEC ON Flusso_RDA TO PUBLIC


