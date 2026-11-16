"use strict";

const roomSearch = document.getElementById("room-search");
const roomSections = document.querySelectorAll("[data-room-section]");

roomSearch.addEventListener("input", () => {
  const terms = roomSearch.value.toLocaleLowerCase().trim().split(/\s+/).filter(Boolean);

  roomSections.forEach((section) => {
    const cards = section.querySelectorAll(".room-card");
    let matches = 0;

    cards.forEach((card) => {
      const searchable = `${card.dataset.roomName} ${card.dataset.roomDescription}`.toLocaleLowerCase();
      const visible = terms.every((term) => searchable.includes(term));
      card.classList.toggle("d-none", !visible);
      if (visible) matches++;
    });

    const noMatches = section.querySelector("[data-no-matches]");
    if (noMatches) noMatches.classList.toggle("d-none", matches > 0);
  });
});
