<link rel="SHORTCUT ICON" href='../images/icon.png' />
<title>SAJELCO</title>
<?php

include('connection.php');
$cou=0;
$billmonth = $_GET['billmonth'];
$billyear = $_GET['billyear'];
$route = $_GET['route'];

$monthNum  = $billmonth;
$dateObj   = DateTime::createFromFormat('!m', $monthNum);
$monthwords = $dateObj->format('F');

$conn = sqlsrv_connect( $serverName, $connectionInfo);  
$tsql = "SELECT DISTINCT * FROM route_codes where rcode = '$route'";   
$stmt = sqlsrv_query( $conn, $tsql);  
if ( $stmt ){  
 	echo '<br>';
}   
else{  
	echo "Error in statement execution.\n";  
	die( print_r( sqlsrv_errors(), true));  
}
$row = sqlsrv_fetch_array( $stmt, SQLSRV_FETCH_NUMERIC);

		$rcode = $row[0];
		$rnum = $row[1];	
		$rdesc = $row[2];
		$arr = str_split($rcode,strlen($rcode)/2);

		$town = $arr[0];
		$brgy = $arr[1];

//$tsql_col_id = "SELECT TOP(1) col_biller_id FROM bills WHERE billMonth=$billmonth AND billYear=$billyear AND accNo LIKE '%-$route-%'";   
$tsql_col_id = "SELECT TOP(1) col_biller_id,date_created FROM bills WHERE billMonth=$billmonth AND billYear=$billyear AND accNo LIKE '%-$route-%' and billNo <> 0";
$stmt_col_id = sqlsrv_query( $conn, $tsql_col_id);  
if ( $stmt_col_id ){  
 	echo '<br>';
}   
else{  
	echo "Error in statement execution.\n";  
	die( print_r( sqlsrv_errors(), true));  
}
$row_col_id = sqlsrv_fetch_array( $stmt_col_id, SQLSRV_FETCH_NUMERIC);
	$col_id = $row_col_id[0];

$tsql_col_name = "SELECT CONCAT(fname,' ',RTRIM(mname),' ',LTRIM(lname) ) as nyme from colltel_id WHERE id=$col_id";   
$stmt_col_name = sqlsrv_query( $conn, $tsql_col_name);  
if ( $stmt_col_name ){  
 	echo '<br>';
}   
else{  
	echo "Error in statement execution.\n";  
	die( print_r( sqlsrv_errors(), true));  
}
$row_col_name = sqlsrv_fetch_array( $stmt_col_name, SQLSRV_FETCH_NUMERIC);
	$col_name = $row_col_name[0];
	/*$date_ngaun = date('F j, Y');*/
	$date_ngaun = $row_col_id[1]->format('m-d-Y');

?>
<style>
@media print {
 
#back,#printCheckDate,#print{ display: none; }
	table { page-break-after:auto }
	tr    { page-break-inside:avoid; page-break-after:auto }
	td    { page-break-inside:avoid; page-break-after:auto }
	thead { display:table-header-group }
	tfoot { display:table-footer-group }
}
</style>
<link rel="stylesheet" href="bootstrap/dist/css/bootstrap.min.css">
<script src="bootstrap/dist/js/bootstrap.min.js"></script>
<div>

		<div class="col-xs-7" style="text-align: center;width: 100%;">
			<h5 >SAN JOSE CITY ELECTRIC COOPERATIVE</h5>
			<h6 >Abar 1st San Jose City</h6>
			 <span style="font-weight: 900;font-size: 12px"> Meter Readings- <?php echo $monthwords.' '.$billyear;  ?></span>
		</div>
		<br>
		<div class="col-xs-12" style="font-size: 10px;">	
			<!-- <div width="100%"> 
				<div >
					<span>AREA: 01 </span>
					<span>TOWN: <?php echo $town;?> </span>
					<span>SAN JOSE </span>
				</div>
				
			</div> 
			
			<div> 
				<div >
					<span>BRGY: <?php echo $brgy.'   '.$rdesc;?></span>
				</div>
				
			</div>-->
			<div style="width: 100%;">	
			
				<table class=" table-sm" style="font-size: 12;width: 100%;margin-top: 20px;">
					<tr>
						<td class="text-left">
							AREA : 01 TOWN : <?php echo $town;?> SAN JOSE CITY
						</td>
						
						<td class="text-right">
							DATE : <?php echo $date_ngaun  ?>
						</td>
						
					</tr>
					<tr>
						<td>
							&nbsp;
						</td>
						<td>
							&nbsp;
						</td>
					</tr>
					<tr>
						<td>
							&nbsp;
						</td>
						<td>
							&nbsp;
						</td>
					</tr>
					<tr>
						<td class="text-left">
							BRGY : <?php echo $brgy.'   '.$rdesc;?>
						</td>
						
						<td class="text-right">
							COLLECTOR : <?php echo $col_name;?>
						</td>
						
					</tr>
				</table>
			</div>

		</div>
		 <div class="col-xs-12" style="font-size: 10px;">	
			<table class="table table-bordered " style="width:100%;margin-top:1%;font-size: 12px;" >
				<thead>
				<tr>
						<th class="text-center">NO</th>
						<th class="text-left">NAME</th>
						<th class="text-center">MSNO</th>	
						<th class="text-center">RC</th>	
						<th class="text-center">ACCOUNT #</th>	
						<th class="text-center">PRES</th>
						<th class="text-center">PREV</th>
						<!-- <th class="text-center">KWHused</th> -->
						<!-- <th class="text-center">CUR_AMT</th> -->
						<th class="text-center">STATUS</th>
						<!-- <th class="text-center"> COL/BIL</th> -->
				</tr>
				</thead> 
				<tbody>  
	<?php
		  
				$tsql1 = "SELECT * FROM
					(SELECT accNo,CONCAT(fname,' ',mname,' ',lname,' ',n_suffix) AS neym, contypeCode, mSerialNo,company,a_suffix FROM consumer_data WHERE accNo LIKE '%-$route-%')c
					LEFT JOIN
					(SELECT accno, billMonth, billYear, ISNULL(previousMeterReading,0.00) as prev, ISNULL(presentMeterReading,0.00) AS pres, kWhUsed, ISNULL(col_biller_id,'') as col,ISNULL(current_amount,0.00) as cur FROM bills WHERE billMonth=$billmonth AND billYear=$billyear)b
					ON c.accNo=b.accNo
					LEFT JOIN
					(SELECT DISTINCT accno, ISNULL(reason,'') as reason FROM android_remarks  WHERE accno LIKE '%-$route-%' AND ( (MONTH(date_created)=$billmonth) and (YEAR(date_created)=$billyear) ) )r
					ON c.accNo=r.accno ORDER BY c.accNo ASC";   
				$stmt1 = sqlsrv_query( $conn, $tsql1);  
				if ( $stmt1 ){  
				}   
				else{  
					echo "Error in statement execution.\n";  
					die( print_r( sqlsrv_errors(), true));  
				}
				while( $row1 = sqlsrv_fetch_array( $stmt1, SQLSRV_FETCH_NUMERIC)) 
				{ $cou++;
					if ($row1[4] == null || $row1[4] == '') {
						$account_name = $row1[1];
					}else{
						$account_name = $row1[4].' C/O '.$row1[1];
					}

					if ($row1[5] == null || $row1[5] == '') {
						$account_name = $account_name;
					}else{
						$account_name = $account_name.' ( '.$row1[5].' ) ';
					}
					?>
									<tr>
										<td class="text-center"> 
											<?php echo $cou; ?> 
										</td> 
										<td class="text-left"> 
											<?php echo $account_name;?> 
										</td> 
										<td class="text-center"> 
											<?php echo $row1[3]; ?> 
										</td> 
										<td class="text-center"> 
											<?php echo $row1[2]; ?>
										</td>
										<td class="text-center" style="width:100px;font-weight: 700"> 
											<?php echo $row1[0]; ?>
										</td>
										<td class="text-center" style="font-weight: 700"> 
											<?php echo $row1[10]; ?>
										</td>
										<td class="text-center"> 
											<?php 

											if($row1[9] != null){
												 echo $row1[9]; 
											}else{

												$tsql3 = "SELECT ereading_prev from android_temp_reading where acc_no = '$row1[0]' 
												and billing_month = $billmonth and billing_year = $billyear";   
												$stmt3 = sqlsrv_query( $conn, $tsql3); 
												while( $row3 = sqlsrv_fetch_array( $stmt3, SQLSRV_FETCH_NUMERIC)) 
												{	echo $row3[0]; }
											}

											?>
										</td>
										<!-- <td class="text-center"> 
											<?php echo $row1[9]; ?>
										</td>
										<td class="text-center"> 
											<?php echo $row1[11]; ?>
										</td> -->
										<td class="text-center"> 
											<?php echo $row1[15]; ?>
										</td>
										<!-- <td class="text-center"> 
											<?php echo $row1[10]; ?>
										</td> -->
					
									</tr>
						<?php	} ?>
	
						
				</tbody>
			</table>


			
	</div>
	<div style="margin-top: 5%;margin-bottom: 1%;float:right">
		<input type="button" value="BACK" id="back"  onclick="back()" class="btn btn-primary btn-sm btn- hvr-pop">
		<input type="button" value="PRINT" id="print" onclick="printRpt()" class="btn btn-primary btn-sm btn- hvr-pop">
	</div>

</div>
	<script type="text/javascript">
		function printRpt(){
			
			window.print();
		}
		function back(){
			setTimeout(window.close,0);
		}
	</script>		

