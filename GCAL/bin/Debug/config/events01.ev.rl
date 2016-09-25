GCFestivalBook:Events in the Pastimes of the Lord and His Associates|1|1
GCFestivalTithiMasa:|Aksaya Trtiya. Candana Yatra starts. (Continues for 21 days)|1|0|1|-10000|1|18|0|17|0
GCFestivalTithiMasa:|Srimati Sita Devi (consort of Lord Sri Rama) -- Appearance|1|0|1|-10000|1|19|0|23|0
GCFestivalTithiMasa:|Rukmini Dvadasi|1|0|1|-10000|1|20|0|26|0
GCFestivalTithiMasa:|Krsna Phula Dola, Salila Vihara|1|0|1|-10000|1|21|0|29|0
GCFestivalTithiMasa:|Sri Sri Radha-Ramana Devaji -- Appearance|1|0|1|-10000|1|22|0|29|0
GCFestivalTithiMasa:|Ganga Puja|1|0|1|-10000|1|23|0|24|1
GCFestivalTithiMasa:|Panihati Cida Dahi Utsava|1|0|1|-10000|1|24|0|27|1
GCFestivalTithiMasa:|Snana Yatra|1|0|1|-10000|1|25|0|29|1
GCFestivalTithiMasa:|Guru (Vyasa) Purnima|1|0|1|-10000|1|26|0|29|2
GCFestivalTithiMasa:|Radha Govinda Jhulana Yatra begins|1|0|1|-10000|1|27|0|25|3
GCFestivalTithiMasa:|Jhulana Yatra ends|1|0|1|-10000|1|28|0|29|3
GCFestivalTithiMasa:|Ananta Caturdasi Vrata|1|0|1|-10000|1|29|0|28|4
GCFestivalTithiMasa:|Sri Visvarupa Mahotsava|1|0|1|-10000|1|30|0|29|4
GCFestivalTithiMasa:|Ramacandra Vijayotsava|1|0|1|-10000|1|31|0|24|5
GCFestivalTithiMasa:|Sri Krsna Saradiya Rasayatra|1|0|1|-10000|1|32|0|29|5
GCFestivalTithiMasa:|Appearance of Radha Kunda, snana dana|1|0|1|-10000|1|33|0|7|6
GCFestivalTithiMasa:|Bahulastami|1|0|1|-10000|1|34|0|7|6
GCFestivalTithiMasa:|Dipa dana, Dipavali, (Kali Puja)|1|0|1|-10000|1|35|0|14|6
GCFestivalTithiMasa:|Bali Daityaraja Puja|1|0|1|-10000|1|36|0|15|6
GCFestivalTithiMasa:|Gopastami, Gosthastami|1|0|1|-10000|1|37|0|22|6
GCFestivalTithiMasa:|Sri Krsna Rasayatra|1|0|1|-10000|1|38|0|29|6
GCFestivalTithiMasa:|Tulasi-Saligrama Vivaha (marriage)|1|0|1|-10000|1|39|0|29|6
GCFestivalTithiMasa:|Katyayani vrata begins|1|0|1|-10000|1|40|0|0|7
GCFestivalTithiMasa:|Odana sasthi|1|0|1|-10000|1|41|0|20|7
GCFestivalTithiMasa:|Advent of Srimad Bhagavad-gita|1|0|1|-10000|1|42|0|25|7
GCFestivalTithiMasa:|Katyayani vrata ends|1|0|1|-10000|1|43|0|29|7
GCFestivalTithiMasa:|Sri Krsna Pusya Abhiseka|1|0|1|-10000|1|44|0|29|8
GCFestivalTithiMasa:|Vasanta Pancami|1|0|1|-10000|1|45|0|19|9
GCFestivalTithiMasa:|Bhismastami|1|0|1|-10000|1|46|0|22|9
GCFestivalTithiMasa:|Sri Krsna Madhura Utsava|1|0|1|-10000|1|47|0|29|9
GCFestivalTithiMasa:|Siva Ratri|1|0|1|-10000|1|48|0|13|10
GCFestivalTithiMasa:|Damanakaropana Dvadasi|1|0|1|-10000|1|49|0|26|11
GCFestivalTithiMasa:|Sri Balarama Rasayatra|1|0|1|-10000|1|50|0|29|11
GCFestivalTithiMasa:|Sri Krsna Vasanta Rasa|1|0|1|-10000|1|51|0|29|11
GCFestivalSpecial:|Go Puja. Go Krda. Govardhana Puja.|1|0|1|-10000|1|6|0|6|6
BeginScript
(set a0 0)
(set a1 0)
(set a2 0)
(set d0 (day -1))
(set d1 (day 0))
(set d2 (day 1))
(if (!= d1.astro.masa 6) then (return 0))
(if (== d0.ksayaTithi 15) then (return 1))
(if (== d0.astro.tithi 15) then (set a0 16))
(if (== d1.astro.tithi 15) then (set a1 16))
(if (== d2.astro.tithi 15) then (set a2 16))
(if (== a1 0) then (return 0))
(if (and (< a0 a1) (> a1 a2)) then (return 1))
(if (< d0.moonRiseSec d0.sunRiseSec) then (set a0 (+ a0 8)))
(if (< d1.moonRiseSec d1.sunRiseSec) then (set a1 (+ a1 8)))
(if (< d2.moonRiseSec d2.sunRiseSec) then (set a2 (+ a2 8)))
(if (or (> a0 a1) (> a2 a1)) then (return 0))
(return 1)
EndScript
GCFestivalTithiMasa:|Ratha Yatra|1|0|1|-10000|1|11|0|16|2
GCFestivalTithiMasa:|Gundica Marjana|1|0|1|-10000|1|5|-1|16|2
GCFestivalTithiMasa:|Return Ratha (8 days after Ratha Yatra)|1|0|1|-10000|1|3|8|16|2
GCFestivalTithiMasa:|Hera Pancami (4 days after Ratha Yatra)|1|0|1|-10000|1|4|4|16|2
GCFestivalTithiMasa:|Festival of Jagannatha Misra|1|0|1|-10000|1|14|1|29|10
GCFestivalEkadasi:|First day of Bhisma Pancaka|1|0|1|-10000|1|58|0|6|1
GCFestivalMasaDay:|Last day of Bhisma Pancaka|1|0|1|-10000|1|59|0|29|6
