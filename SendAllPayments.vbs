Dim o
Set o = CreateObject("MSXML2.XMLHTTP")
o.open "POST", "https://www.moolslife.com/Payment/Notify/SendAllPayments/", False
o.send