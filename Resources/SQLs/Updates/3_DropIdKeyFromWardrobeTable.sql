CREATE TABLE `user_wardrobe` (
    `user_id` INT(10) UNSIGNED NOT NULL,
    `slot_id` INT(10) UNSIGNED NOT NULL,
    `look` VARCHAR(120) NOT NULL COLLATE 'latin1_swedish_ci',
     `gender` ENUM('F','M') NOT NULL DEFAULT 'M' COLLATE 'latin1_swedish_ci',
     UNIQUE INDEX `user_id` (`user_id`, `slot_id`) USING BTREE
)
    COLLATE='latin1_swedish_ci'
    ENGINE=InnoDB
;
