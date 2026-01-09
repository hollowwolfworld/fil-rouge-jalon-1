
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