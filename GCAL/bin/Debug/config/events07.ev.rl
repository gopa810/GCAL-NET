GCFestivalBook:Caturmasya (Pratipat System)|7|1
GCFestivalMasaDay:|First month of Caturmasya begins (Pratipat System)|7|0|1|-10000|1|158|0|0|3
GCFestivalMasaDay:|(green leafy vegetable fast for one month)|7|0|1|-10000|1|159|0|0|3
GCFestivalMasaDay:|Last day of the first Caturmasya month|7|0|1|-10000|1|160|-1|0|4
GCFestivalMasaDay:|Second month of Caturmasya begins (Pratipat System)|7|0|1|-10000|1|161|0|0|4
GCFestivalMasaDay:|(yogurt fast for one month)|7|0|1|-10000|1|162|0|0|4
GCFestivalMasaDay:|Last day of the second Caturmasya month|7|0|1|-10000|1|163|-1|0|5
GCFestivalMasaDay:|Third month of Caturmasya begins (Pratipat System)|7|0|1|-10000|1|164|0|0|5
GCFestivalMasaDay:|(milk fast for one month)|7|0|1|-10000|1|165|0|0|5
GCFestivalMasaDay:|Last day of the third Caturmasya month|7|0|1|-10000|1|166|-1|0|6
GCFestivalMasaDay:|Fourth month of Caturmasya begins (Pratipat System)|7|0|1|-10000|1|167|0|0|6
GCFestivalMasaDay:|(urad dal fast for one month)|7|0|1|-10000|1|168|0|0|6
GCFestivalMasaDay:|Last day of the fourth Caturmasya month|7|0|1|-10000|1|169|-1|0|7
GCFestivalSpecial:|First month of Caturmasya continues|7|0|1|-10000|1|6|0|3|3
BeginScript
(set d0 (day -1))
(set d1 (day 0))
(if (and (== d0.astro.masa 12) (== d1.astro.masa 3)) then (return 1))
(return 0)
EndScript
GCFestivalSpecial:|Second month of Caturmasya continues|7|0|1|-10000|1|6|0|4|4
BeginScript
(set d0 (day -1))
(set d1 (day 0))
(if (and (== d0.astro.masa 12) (== d1.astro.masa 4)) then (return 1))
(return 0)
EndScript
GCFestivalSpecial:|Third month of Caturmasya continues|7|0|1|-10000|1|6|0|5|5
BeginScript
(set d0 (day -1))
(set d1 (day 0))
(if (and (== d0.astro.masa 12) (== d1.astro.masa 5)) then (return 1))
(return 0)
EndScript
GCFestivalSpecial:|Fourth month of Caturmasya continues|7|0|1|-10000|1|6|0|6|6
BeginScript
(set d0 (day -1))
(set d1 (day 0))
(if (and (== d0.astro.masa 12) (== d1.astro.masa 6)) then (return 1))
(return 0)
EndScript
