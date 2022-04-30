
ALTER TABLE `user_wardrobe`
    DROP COLUMN `id`,
    DROP INDEX `slot_id`,
    DROP PRIMARY KEY,
    DROP INDEX `user_id`,
    ADD UNIQUE INDEX `user_id` (`user_id`, `slot_id`) USING BTREE;