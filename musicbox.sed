1s/.*/<music>/p
$s/.*/<\/music>/p
/^p=/ { s/^p=\(.*\)/\t<nota p='\1' /p; b end }
/^t=/ { s/^t=\(.*\)/\t\tt='\1'\/>/p; b end }
/^[visa]=/ b end
s/^\(.*\)=\(.*\)/\t\<\1>\2<\/\1>/p

:end