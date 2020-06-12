$binobj = @(Get-ChildItem .\ -Recurse -Depth 3 -Attributes Directory -Include bin, obj)
foreach ($d in $binobj)
{
    Write-Host "Removing $($d.FullName)"
    Remove-Item $d.FullName -Recurse -Force -ErrorAction Ignore
}