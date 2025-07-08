Test case 2 - Replace a book

Swagger -https://bookstore.toolsqa.com/swagger

Host -https://bookstore.toolsqa.com

Preconditions:
- A user with a randomly generated username and password is created and authorized.

| Steps | Expected Results |
|-------|------------------|
|1 Get all books sendingGET /BookStore/v1/Books. |Status code 200. The book list contains books.|
|2 Add the first book from the book list from the previous response to the user's book list sendingPOST /BookStore/v1/Books.| Status code 201.|
|3 Get the user by its id sendingGET /Account/v1/User/{userId}.|Status code 200. The user's book list contains only one book. The book in the user's book list matches the book added in the previous step.|
|4 Replace the book from the user's book list with the second book from the book list from the previous response sendingPOST /BookStore/v1/Books.| Status code 200.|
|5 Get the user by its id sendingGET /Account/v1/User/{userId}. |Status code 200. The user's book list contains only one book. The book in the user's book list was replaced and matches the book from the previous step.|

Postconditions:
- Delete the created user.

