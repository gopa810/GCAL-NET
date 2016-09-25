GCFestivalBook:Caturmasya (Purnima System)|8|0
GCFestivalMasaDay:|First month of Caturmasya begins (Purnima System)|8|0|1|-10000|1|174|0|29|2
GCFestivalMasaDay:|(green leafy vegetable fast for one month)|8|0|1|-10000|1|175|0|29|2
GCFestivalMasaDay:|Last day of the first Caturmasya month|8|0|1|-10000|1|176|-1|29|3
GCFestivalMasaDay:|Second month of Caturmasya begins (Purnima System)|8|0|1|-10000|1|177|0|29|3
GCFestivalMasaDay:|(yogurt fast for one month)|8|0|1|-10000|1|178|0|29|3
GCFestivalMasaDay:|Last day of the second Caturmasya month|8|0|1|-10000|1|179|-1|29|4
GCFestivalMasaDay:|Third month of Caturmasya begins (Purnima System)|8|0|1|-10000|1|180|0|29|4
GCFestivalMasaDay:|(milk fast for one month)|8|0|1|-10000|1|181|0|29|4
GCFestivalMasaDay:|Last day of the third Caturmasya month|8|0|1|-10000|1|182|-1|29|5
GCFestivalMasaDay:|Fourth month of Caturmasya begins (Purnima System)|8|0|1|-10000|1|183|0|29|5
GCFestivalMasaDay:|(urad dal fast for one month)|8|0|1|-10000|1|184|0|29|5
GCFestivalMasaDay:|Last day of the fourth Caturmasya month|8|0|1|-10000|1|185|-1|29|6
GCFestivalSpecial:|First month of Caturmasya continues|8|0|1|-10000|1|6|0|3|3
BeginScript
(set d0 (day -1))
(set d1 (day 0))
(if (and (== d0.astro.masa 12) (== d1.astro.masa 3)) then (return 1))
(return 0)
EndScript
GCFestivalSpecial:|Second month of Caturmasya continues|8|0|1|-10000|1|6|0|4|4
BeginScript
(set d0 (day -1))
(set d1 (day 0))
(if (and (== d0.astro.masa 12) (== d1.astro.masa 4)) then (return 1))
(return 0)
EndScript
GCFestivalSpecial:|Third month of Caturmasya continues|8|0|1|-10000|1|6|0|5|5
BeginScript
(set d0 (day -1))
(set d1 (day 0))
(if (and (== d0.astro.masa 12) (== d1.astro.masa 5)) then (return 1))
(return 0)
EndScript
GCFestivalSpecial:|Fourth month of Caturmasya continues|8|0|1|-10000|1|6|0|6|6
BeginScript
(set d0 (day -1))
(set d1 (day 0))
(if (and (== d0.astro.masa 12) (== d1.astro.masa 6)) then (return 1))
(return 0)
EndScript
