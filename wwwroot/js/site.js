
//aparition du menu burger

icons.addEventListener("click", () => {
    menu.classList.toggle("active");
    header.classList.toggle("active");
})

//carouselle d'img

const buttons = document.querySelectorAll(".btnCarouselle");

const slides = document.querySelectorAll(".slide");

buttons.forEach((button) => {
    button.addEventListener("click", (e) => {
        const calcnextslide = e.target.id === "next" ? 1 : -1;
        const slideActive = document.querySelector(".active")

        newIndex = calcnextslide + [...slides].indexOf(slideActive);

        if (newIndex < 0) {
            newIndex = [...slides].length - 1;
        }

        if (newIndex >= [...slides].length) {
            newIndex = 0;
        }

        slides[newIndex].classList.add("active");

        slideActive.classList.remove("active");

    })
})
//gestion de l'api fetch
async function DateLivraison(zipCode) {
    console.log(zipCode);
    const url = "https://api-filrouge.2isa.eu/api/v1/shippingaddress?zipcode=" + zipCode;
    try {
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`Response status: ${response.status}`);
        }
        const result = await response.json();
        console.log("distance : " + result.distanceKM + "KM");

        let nbKm = 0;
        let nbJourPrepa = 1; 
        let jour = 0; 
        nbKm = result.distanceKM / 30;


        jour = nbJourPrepa + nbKm;
        jour = Math.round(jour)
       

        let ajoutFichier = document.getElementById('jour').textContent = jour + " jours";


    } catch (error) {
        console.error(error.Message);
    }
}