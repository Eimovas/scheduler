When employee can work more than the surgery needs:
 employee availability:   start - 8, finish - 17
 1. practice works :         start - 8, finish - 12
 2. practice works :         start - 9, finish - 12
 3. practice works :         start - 11, finish - 12
 4. practice works :         start - 13, finish - 17
 5. practice works :         start - 13, finish - 16
 6. practice works :         start - 15, finish - 17

 1. rule -> pw.start - e.start = 0 && pw.finish - e.finish < 0  
 2. rule -> pw.start - e.start > 0 && pw.finish - e.finish < 0 
 3. rule -> rule 2. 
 4. rule -> pw.start - e.start > 0 && pw.finish - e.finish = 0 
 5. rule -> rule 2.
 6. rule -> rule 4.

When employee can work less than the surgery needs:
employee availability:		 start - 9, finish - 12

 1. practice works :         start - 8, finish - 12
 2. practice works :         start - 9, finish - 13
 3. practice works :         start - 8, finish - 13
 
 1. rule -> pw.start - e.start < 0 && pw.finish - e.finish = 0
 2. rule -> pw.start - e.start = 0 && pw.finish - e.finish > 0
 3. rule -> pw.start - e.start < 0 && pw.finish - e.finish > 0