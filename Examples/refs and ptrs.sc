var iPtr
var somePtr

loop var i = 0, i 100_000_000 <, i = i 1 +
    PrintLn(i) # print i
    iPtr = ptr i # get a pointer to variable i
    iPtr -> i 2 + # set iPtr by it's pointer
    PrintLn(i) # print i (increased by two)

    somePtr = 2 # i pointer
    somePtr -> ref iPtr 3 + # get value from pointer iPtr, add 2, set it by somePtr pointer
    PrintLn(i) # print i (increased by three)
    
    PrintLn('') # new line
end