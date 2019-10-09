# IRL-Royale

## Workflow for Pushing Changes
After completing changes IN YOUR BRANCH:
 - Add and commit all relevant files

Update dev:
 - `git checkout dev`
 - `git pull dev`
 
Update your branch to have the new dev changes:
 - `git checkout [YOUR BRANCH]`
 - `git merge dev`
 - fix any merge conflicts
 - test to make sure nothing broke
 
Squash your commits:
 - `git rebase -i HEAD~[NUMBER OF COMMITS]` OR `git rebase -i [SHA HASH OF FIRST COMMIT]`

Push to dev
 - `git checkout dev`
 - `git merge [YOUR BRANCH]`
 - `git push'
