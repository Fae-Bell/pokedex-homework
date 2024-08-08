# Prompt
Pokemon have magically come to life and are running rampant across the globe! Park Rangers have collected extensive data on each Pokemon and need a way to present and share it with everyone.

We must build a web application that allows users to browse through the pokemon and view details on each. Specifically, this should be a React app that calls into a C# API. 

The Park Rangers’ data has been recorded in the provided pokemon.json file for the API to use.

# Acceptance Criteria
The app should open to a dashboard page that includes summary data at the top and a table of available pokemon below. Clicking on a pokemon name from within the table should link to a details page for that pokemon. 

- Dashboard Page
  - Summary section
    - Lists total pokemon species
    - Lists counts of pokemon per each pokemon type
    - Lists counts of pokemon per each generation
  - Table section
    - Displays 25 pokemon at a time with paging.
    - Table Columns
      - Number
      - Name
      - Generation
      - Height
      - Weight
      - Type 1
      - Type 2
      - Moves count
    - (Optional) The user should be able to filter via the following properties:
      - Number (freetext search)
      - Name (freetext search)
      - Type (dropdown)
      - Generation (dropdown)
      - Move (freetext search)
    - (Optional) The user should be able to sort each column.

- Details Page
  - Lists all data set properties of the Pokemon
    - The Evolution To/From should link to that Pokemon’s page
    - The image url should be rendered as the image

# What we are looking for
- A local react project with setup/execution instructions.
- A local C# project with setup/execution instructions.