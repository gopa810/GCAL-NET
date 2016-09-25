GCFestivalBook:Appearance Days of the Lord and His Incarnations|0|1
GCFestivalTithiMasa:Lord Nrsimhadeva|Nrsimha Caturdasi: Appearance of Lord Nrsimhadeva|0|7|1|-10000|1|1|0|28|0
GCFestivalTithiMasa:Lord Balarama|Lord Balarama -- Appearance|0|7|1|-10000|1|2|0|29|3
GCFestivalTithiMasa:Srimati Radharani|Radhastami: Appearance of Srimati Radharani|0|0|1|-10000|1|3|0|22|4
GCFestivalTithiMasa:Vamanadeva|Sri Vamana Dvadasi: Appearance of Lord Vamanadeva|0|7|1|-10000|1|4|0|26|4
GCFestivalTithiMasa:Advaita Acarya|Sri Advaita Acarya -- Appearance|0|7|1|-10000|1|5|0|21|9
GCFestivalTithiMasa:Varahadeva|Varaha Dvadasi: Appearance of Lord Varahadeva|0|7|1|-10000|1|6|0|26|9
GCFestivalTithiMasa:Sri Nityananda|Nityananda Trayodasi: Appearance of Sri Nityananda Prabhu|0|7|1|-10000|1|7|0|27|9
GCFestivalSpecial:Sri Krsna|Sri Krsna Janmastami: Appearance of Lord Sri Krsna|0|7|1|-10000|1|10|0|4|4
GCFestivalRelated:|Nandotsava|1|0|1|-10000|1|13|1
GCFestivalRelated:Srila Prabhupada|Srila Prabhupada -- Appearance|2|0|1|-10000|1|15|1
BeginScript
(set a0 0)
(set a1 0)
(set a2 0)
(set d0 (day -1))
(set d1 (day 0))
(set d2 (day 1))
(if (== d0.ksayaTithi 7) then (return 1))
(if (== d0.astro.tithi 7) then (set a0 16))
(if (== d1.astro.tithi 7) then (set a1 16))
(if (== d2.astro.tithi 7) then (set a2 16))
(if (== a1 0) then (return 0))
(if (and (< a0 a1) (> a1 a2)) then (return 1))
(if (== d0.astro.naksatra 3) then (set a0 (+ a0 8)))
(if (== d1.astro.naksatra 3) then (set a1 (+ a1 8)))
(if (== d2.astro.naksatra 3) then (set a2 (+ a2 8)))
(if (and (< a0 a1) (> a1 a2)) then (return 1))
(if (or  (> a0 a1) (> a2 a1)) then (return 0))
(if (== d0.midnightNaksatra 3) then (set a0 (+ a0 4)))
(if (== d1.midnightNaksatra 3) then (set a1 (+ a1 4)))
(if (== d2.midnightNaksatra 3) then (set a2 (+ a2 4)))
(if (and (< a0 a1) (> a1 a2)) then (return 1))
(if (or  (> a0 a1) (> a2 a1)) then (return 0))
(if (== d0.date.weekday 0) then (set a0 (+ a0 2)))
(if (== d1.date.weekday 0) then (set a1 (+ a1 2)))
(if (== d2.date.weekday 0) then (set a2 (+ a2 2)))
(if (== d0.date.weekday 2) then (set a0 (+ a0 2)))
(if (== d1.date.weekday 2) then (set a1 (+ a1 2)))
(if (== d2.date.weekday 2) then (set a2 (+ a2 2)))
(if (== d2.astro.tithi 7) then (set a1 (+ a1 1)))
(if (and (< a0 a1) (> a1 a2)) then (return 1))
(return 0)
EndScript
GCFestivalTithiMasa:Sri Krsna Caitanya Mahaprabhu|Gaura Purnima: Appearance of Sri Caitanya Mahaprabhu|0|7|1|-10000|1|12|0|29|10
GCFestivalSpecial:Sri Ramacandra|Rama Navami: Appearance of Lord Sri Ramacandra|0|7|1|-10000|1|9|0|11|11
BeginScript
(set d0 (day -1))
(set d1 (day 0))
(set d2 (day 1))
(set d3 (day 2))
(if (== d0.ksayaTithi 23) then (return 1))
(if (== d0.astro.tithi 23) then (return 0))
(if (and (== d2.astro.tithi 23) (>= d3.fastType 518)) then (return 1))
(if (and (== d1.astro.tithi 23) (< d3.fastType 518)) then (return 1))
(return 0)
EndScript
GCFestivalSankranti:|Some Kanya|0|0|1|-10000|1|305|-5|5
GCFestivalTithiMasa:|Moje narodky|0|0|1|-10000|1|497|0|22|0
GCFestivalRelated:|After 4 days|0|0|1|-10000|1|498|4
