


CREATE Procedure [dbo].[Esiti_Ordini_SAP]

(

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
@DBfetch_status int

--none procedura
set @PROCname = 'Esiti_Ordini_SAP'

--conservo il file degli esiti originale
INSERT INTO RDA_Esiti (Mandante, Societa, Num_Ordine, Posizione, Cod_Errore, Desc_Errore, Data_FileOutput) 
SELECT	Mandante, Societa, Num_Ordine, Posizione, Cod_Errore, Desc_Errore, Data_FileOutput 
FROM    eProcurement.dbo.RDA_Esiti_LastOutput_OnLine 

--escludo i warning in quanto non sono da considerare errori
UPDATE eProcurement.dbo.RDA_Esiti_LastOutput_OnLine SET Cod_Errore = null 
	WHERE LEFT(Cod_Errore,1) = 'W'

--Non inserita in SAP
UPDATE eProcurement.dbo.RdA SET Status = 16   
FROM   eProcurement.dbo.RdA inner join 
(SELECT DISTINCT Mandante, Societa, Num_Ordine 
		FROM  eProcurement.dbo.RDA_Esiti_LastOutput_OnLine 
		WHERE  (Cod_Errore IS NOT NULL)) AS con_errori  
		on RdA.NOrdSAP = con_errori.Num_Ordine 


--Scrivo nel Log
INSERT INTO eProcurement.dbo.LogTime_Ciclo ([Data],[Rch_Progr],[Rch_ID],[PageID],[Matricola],[StatoID],[StatoIntID],[UOCodeNext],[RuoloNext],[IstIDNext]) 
SELECT GETDATE(), LogTime_Ciclo.Rch_Progr, LogTime_Ciclo.Rch_ID, LogTime_Ciclo.PageID, 'BBSISTE' AS Matricola, 
		16 as StatoID, 16 as StatoIntID, LogTime_Ciclo.UOCodeNext, LogTime_Ciclo.RuoloNext, LogTime_Ciclo.IstIDNext 
FROM (SELECT Rch_ID, MAX(IDTC) AS ID FROM eProcurement.dbo.LogTime_Ciclo GROUP BY Rch_ID) AS max_id INNER JOIN 
	eProcurement.dbo.LogTime_Ciclo ON max_id.ID = LogTime_Ciclo.IDTC INNER JOIN 
	eProcurement.dbo.RdA ON LogTime_Ciclo.Rch_ID = RdA.GUID INNER JOIN 
	(SELECT DISTINCT Mandante, Societa, Num_Ordine FROM eProcurement.dbo.RDA_Esiti_LastOutput_OnLine 
	WHERE (Cod_Errore IS NOT NULL)) AS con_errori ON RdA.NOrdSAP = con_errori.Num_Ordine 


--Inserita in SAP
UPDATE eProcurement.dbo.RdA 
SET   Status = 20 
FROM  eProcurement.dbo.RdA INNER JOIN 
(SELECT  no_errori.Mandante, no_errori.Societa, no_errori.Num_Ordine 
 FROM  
	(SELECT DISTINCT Mandante, Societa, Num_Ordine FROM eProcurement.dbo.RDA_Esiti_LastOutput_OnLine WHERE      (Cod_Errore IS NULL)) AS no_errori LEFT OUTER JOIN 
	(SELECT DISTINCT Mandante, Societa, Num_Ordine FROM eProcurement.dbo.RDA_Esiti_LastOutput_OnLine WHERE      (Cod_Errore IS NOT NULL)) AS con_errori ON no_errori.Num_Ordine = con_errori.Num_Ordine 
 WHERE (con_errori.Num_Ordine IS NULL)) AS ord ON RdA.NOrdSAP = ord.Num_Ordine 

--Modifico lo stato della RdE
UPDATE eProcurement.dbo.RdE 
SET   Status = 20 
FROM  eProcurement.dbo.RdE INNER JOIN eProcurement.dbo.RdA ON RdE.RdAID = RdA.GUID INNER JOIN 
(SELECT  no_errori.Mandante, no_errori.Societa, no_errori.Num_Ordine 
	FROM  
		(SELECT DISTINCT Mandante, Societa, Num_Ordine FROM  eProcurement.dbo.RDA_Esiti_LastOutput_OnLine WHERE  (Cod_Errore IS NULL)) AS no_errori LEFT OUTER JOIN 
		(SELECT DISTINCT Mandante, Societa, Num_Ordine FROM  eProcurement.dbo.RDA_Esiti_LastOutput_OnLine WHERE  (Cod_Errore IS NOT NULL)) AS con_errori ON no_errori.Num_Ordine = con_errori.Num_Ordine 
	WHERE (con_errori.Num_Ordine IS NULL)) AS ord ON RdA.NOrdSAP = ord.Num_Ordine 

--Scrivo nel Log
INSERT INTO eProcurement.dbo.LogTime_Ciclo ([Data],[Rch_Progr],[Rch_ID],[PageID],[Matricola],[StatoID],[StatoIntID] ,[UOCodeNext],[RuoloNext],[IstIDNext]) 
SELECT  
	GETDATE(), LogTime_Ciclo.Rch_Progr, LogTime_Ciclo.Rch_ID, LogTime_Ciclo.PageID,  'BBSISTE' AS Matricola,  
	20 as StatoID, 20 as StatoIntID, LogTime_Ciclo.UOCodeNext, LogTime_Ciclo.RuoloNext, LogTime_Ciclo.IstIDNext
FROM 
	(SELECT     Rch_ID, MAX(IDTC) AS ID FROM	eProcurement.dbo.LogTime_Ciclo GROUP BY Rch_ID) AS max_id INNER JOIN 
	eProcurement.dbo.LogTime_Ciclo ON max_id.ID = LogTime_Ciclo.IDTC INNER JOIN 
	eProcurement.dbo.RdA ON LogTime_Ciclo.Rch_ID = RdA.GUID INNER JOIN 
	(SELECT     no_errori.Mandante, no_errori.Societa, no_errori.Num_Ordine FROM 
	(SELECT DISTINCT Mandante, Societa, Num_Ordine FROM eProcurement.dbo.RDA_Esiti_LastOutput_OnLine WHERE (Cod_Errore IS NULL)) AS no_errori LEFT OUTER JOIN 
	(SELECT DISTINCT Mandante, Societa, Num_Ordine FROM eProcurement.dbo.RDA_Esiti_LastOutput_OnLine WHERE (Cod_Errore IS NOT NULL)) AS con_errori ON no_errori.Num_Ordine = con_errori.Num_Ordine 
WHERE      (con_errori.Num_Ordine IS NULL)) AS ord ON RdA.NOrdSAP = ord.Num_Ordine




fine:


--restituisco il valore
set @po_valoreuscita=''


--success
return 0



GRANT EXEC ON Esiti_Ordini_SAP TO PUBLIC

