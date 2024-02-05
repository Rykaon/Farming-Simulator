INCLUDE globals.ink
EXTERNAL PlantSellBuy(PlantToBuy, PlantPrice, SellOrBuy)
VAR PlantName = "Attack"
VAR PlantPrice = 15
 
{ PlayerArgent>15  : ->main | ->pauvre}

=== main ===
Bienvenue au magasin ! Qu'est-ce que je peux faire pour vous ?
-> magasin

=== magasin ===
    + [Je veux acheter une plante !]
        -> buyPlant
    + [Je veux vendre une plante !]
        -> sellPlant
    * [Au revoir !]
        Salut mon pote !
        -> END

=== buyPlant ===
Laquelle voulez-vous acheter ? ({PlayerArgent}$)
            + [Une Rouge ! (15$)]
                -> setUp (PlantName, PlantPrice, false, NbPlanteRouge)
            + [Une Bleu ! (15$)]
                -> setUp (PlantName, PlantPrice, false, NbPlanteBleu)
            + [Une Jaune ! (15$)]
                -> setUp (PlantName, PlantPrice, false, NbPlanteJaune)
                

=== sellPlant ===
Laquelle voulez-vous vendre ?
            + [Une Rouge ! ({NbPlanteRouge})]
                -> setUp (PlantName, PlantPrice, true, NbPlanteRouge)
            + [Une Bleu ! ({NbPlanteBleu})]
                -> setUp (PlantName, PlantPrice, true, NbPlanteBleu)
            + [Une Jaune ! ({NbPlanteJaune})]
                -> setUp (PlantName, PlantPrice, true, NbPlanteJaune)


=== setUp (Name, Price, SellOrBuy, nbExemplaire) ===
    {SellOrBuy == false :
        {PlayerArgent > Price :
            ~ PlayerArgent = PlayerArgent-Price
            ~ nbExemplaire++
            ~ PlantSellBuy(Name, Price, false)
            Merci pour l'achat !
            - else :
            T'as pas assez d'argent mon pote !
       }
       - else :
       {nbExemplaire > 0 :
            ~ PlayerArgent = PlayerArgent+Price
            ~ nbExemplaire--
            ~ PlantSellBuy(Name, Price, true)
            C'est un plaisir de faire affaire avec vous ! ({Price})
            - else :
            Mais ... t'as aucune {Name} !
        }
    }
    ->magasin


=== pauvre ===
Je sais que t'es fauché, casse toi !
->magasin