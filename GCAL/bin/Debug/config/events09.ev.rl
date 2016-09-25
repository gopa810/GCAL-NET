GCFestivalBook:Caturmasya (Ekadasi System)|9|0
GCFestivalEkadasi:|First month of Caturmasya begins (Ekadasi System)|9|0|1|-10000|1|190|0|2|1
GCFestivalEkadasi:|(green leafy vegetable fast for one month)|9|0|1|-10000|1|191|0|2|1
GCFestivalEkadasi:|Last day of the first Caturmasya month|9|0|1|-10000|1|192|-1|3|1
GCFestivalEkadasi:|Second month of Caturmasya begins (Ekadasi System)|9|0|1|-10000|1|193|0|3|1
GCFestivalEkadasi:|(yogurt fast for one month)|9|0|1|-10000|1|194|0|3|1
GCFestivalEkadasi:|Last day of the second Caturmasya month|9|0|1|-10000|1|195|-1|4|1
GCFestivalEkadasi:|Third month of Caturmasya begins (Ekadasi System)|9|0|1|-10000|1|196|0|4|1
GCFestivalEkadasi:|(milk fast for one month)|9|0|1|-10000|1|197|0|4|1
GCFestivalEkadasi:|Last day of the third Caturmasya month|9|0|1|-10000|1|198|-1|5|1
GCFestivalEkadasi:|Fourth month of Caturmasya begins (Ekadasi System)|9|0|1|-10000|1|199|0|5|1
GCFestivalEkadasi:|(urad dal fast for one month)|9|0|1|-10000|1|200|0|5|1
GCFestivalEkadasi:|Last day of the fourth Caturmasya month|9|0|1|-10000|1|201|-1|6|1
GCFestivalSpecial:|First month of Caturmasya continues|9|0|1|-10000|1|6|0|3|3
BeginScript
(set d0 (day -1))
(set d1 (day 0))
(if (and (== d0.astro.masa 12) (== d1.astro.masa 3)) then (return 1))
(return 0)
EndScript
GCFestivalSpecial:|Second month of Caturmasya continues|9|0|1|-10000|1|6|0|4|4
BeginScript
(set d0 (day -1))
(set d1 (day 0))
(if (and (== d0.astro.masa 12) (== d1.astro.masa 4)) then (return 1))
(return 0)
EndScript
GCFestivalSpecial:|Third month of Caturmasya continues|9|0|1|-10000|1|6|0|5|5
BeginScript
(set d0 (day -1))
(set d1 (day 0))
(if (and (== d0.astro.masa 12) (== d1.astro.masa 5)) then (return 1))
(return 0)
EndScript
GCFestivalSpecial:|Fourth month of Caturmasya continues|9|0|1|-10000|1|6|0|6|6
BeginScript
(set d0 (day -1))
(set d1 (day 0))
(if (and (== d0.astro.masa 12) (== d1.astro.masa 6)) then (return 1))
(return 0)
EndScript
