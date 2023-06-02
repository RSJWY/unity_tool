---[ Table 最大值以及返回的索引 ]
function Table_maxn(t)
  local mn=nil;
  for k, v in pairs(t) do
    if(mn==nil) 
    then
      mn=v
    end
    if mn < v 
    then
      mn = v
    end
  end
  return mn
end
tbl = {[1] = 2, [2] = 6, [3] = 34, [26] =5}
print("tbl 最大值：", table_maxn(tbl))
print("tbl 长度 ", #tbl)