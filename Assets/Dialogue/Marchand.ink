INCLUDE globals.ink
EXTERNAL PlantSellBuy(PlantToBuy, PlantPrice, SellOrBuy)
->main

=== main ===
Bienvenue au magasin ! Qu'est-ce que je peux faire pour vous ? #speaker:Le Gros Ratz #portrait:Le Gros Ratz #layout:left
-> magasin

=== magasin ===
    + [Je veux acheter une graine !]
        -> buyPlant
    + [Je veux vendre une graine !]
        -> sellPlant
    * [Au revoir !]
        Salut mon pote !
        -> END

=== buyPlant ===
Laquelle voulez-vous acheter ? ({PlayerArgent}$) #speaker:Le Gros Ratz #portrait:Le Gros Ratz #layout:right
            + [Une Attaque ! (15$)]
                -> setUp (NamePlant1, PricePlant1, false, NbPlant1)
            + [Une Ressort ! (15$)]
                -> setUp (NamePlant2, PricePlant2, false, NbPlant2)
            + [Une Boost ! (15$)]
                -> setUp (NamePlant3, PricePlant3, false, NbPlant3)
                

=== sellPlant ===
Laquelle voulez-vous vendre ? #speaker:Le Gros Ratz #portrait:Le Gros Ratz #layout:left
            + [Une Attaque ! ({NbPlant1})]
                -> setUp (NamePlant1, PricePlant1, true, NbPlant1)
            + [Une Ressort ! ({NbPlant2})]
                -> setUp (NamePlant2, PricePlant2, true, NbPlant2)
            + [Une Boost ! ({NbPlant3})]
                -> setUp (NamePlant3, PricePlant3, true, NbPlant3)


=== setUp (Name, Price, SellOrBuy, nbExemplaire) ===
    {SellOrBuy == false :
        {PlayerArgent > Price :
            ~ PlayerArgent = PlayerArgent-Price
            ~ nbExemplaire++
            ~ PlantSellBuy(Name, Price, false)
            Merci pour l'achat ! #speaker:Le Gros Rat #portrait:GrosRat #layout:left
            - else :
            T'as pas assez d'argent mon pote ! #speaker:Le Gros Rat #portrait:GrosRat #layout:left
       }
       - else :
       {nbExemplaire > 0 :
            ~ PlayerArgent = PlayerArgent+Price
            ~ nbExemplaire--
            ~ PlantSellBuy(Name, Price, true)
            C'est un plaisir de faire affaire avec vous ! ({Price}) #speaker:Le Gros Rat #portrait:GrosRat #layout:left
            - else :
            Mais ... t'as aucune {Name} ! #speaker:Le Gros Rat #portrait:GrosRat #layout:left
        }
    }
    ->magasin


=== pauvre ===
Je sais que t'es fauché, casse toi ! #speaker:Le Gros Ratz #portrait:Le Gros Ratz #layout:left
->magasin